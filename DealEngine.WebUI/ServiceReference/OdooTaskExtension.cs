using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DealEngine.Services.Interfaces; // <-- your interface namespace
using DealEngine.Domain.Entities;


namespace DealEngine.WebUI.ServiceReference
{
    public sealed class OdooTaskExtension : IOdooTaskGateway, IDisposable
    {
        private readonly HttpClient _http;
        private readonly bool _ownsHttp;

        // removed readonly so they can be set from OdooGatewayconnection()
        private string? _api;   // FULL jsonrpc URL, e.g. https://not4profit.online/jsonrpc
        private string? _db;    // e.g. not4profitodoo18
        private string? _login; // e.g. ashuchauhan@verinsure.online
        private string? _key;   // API key
        private int _uid;

        public OdooTaskExtension(HttpClient? http = null)
        {
            _http = http ?? new HttpClient();
            _ownsHttp = http is null;
            _http.Timeout = TimeSpan.FromSeconds(30);
        }

        public void Dispose()
        {
            if (_ownsHttp) _http.Dispose();
        }

        // ---------------- IOdooTaskGateway ----------------

        // Initialize connection + login. Returns uid.
        public async Task<int> OdooGatewayconnection(string endpoint, string db, string login, string key)
        {
            _api = (endpoint ?? throw new ArgumentNullException(nameof(endpoint))).TrimEnd('/');
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _login = login ?? throw new ArgumentNullException(nameof(login));
            _key = key ?? throw new ArgumentNullException(nameof(key));

            EnsureConfig();
            await LoginAsync();
            return _uid;
        }

        //public async Task<int[]> CreateTasksAsync(IEnumerable<OdooTaskSpec> tasks)
        //{
        //    EnsureLoggedIn();

        //    if (tasks is null) throw new ArgumentNullException(nameof(tasks));

        //    var valsList = tasks.Select(t =>
        //    {
        //        var vals = new Dictionary<string, object?>
        //        {
        //            ["name"] = t.Title ?? throw new ArgumentNullException(nameof(t.Title)),
        //            ["project_id"] = t.ProjectId
        //        };
        //        if (!string.IsNullOrWhiteSpace(t.Notes)) vals["description"] = t.Notes;
        //        if (t.Deadline is DateTime d) vals["date_deadline"] = d.ToString("yyyy-MM-dd");
        //        if (t.AssigneeUserId is int u) vals["user_id"] = u;
        //        if (t.TagIds is not null) vals["tag_ids"] = new object[] { new object[] { 6, 0, t.TagIds.ToArray() } };
        //        return (object)vals;
        //    }).ToArray();

        //    // One execute_kw with a list of value dicts → bulk create
        //    var payload = ExecKw("project.task", "create", new object[] { valsList });

        //    return await RpcAsync<int[]>(payload); // returns array of new task IDs
        //}


        public async Task<int> CreateTaskAsync(string title, int projectId, string? notes = null,
                                               DateTime? deadline = null, int? assigneeUserId = null,
                                               IEnumerable<int>? tagIds = null)
        {
            EnsureLoggedIn();

            var vals = new Dictionary<string, object?>
            {
                ["name"] = title ?? throw new ArgumentNullException(nameof(title)),
                ["project_id"] = projectId
            };
            if (!string.IsNullOrWhiteSpace(notes)) vals["description"] = notes;
            if (deadline is DateTime d) vals["date_deadline"] = d.ToString("yyyy-MM-dd");
            if (assigneeUserId is int u) vals["user_id"] = u;
            if (tagIds is not null) vals["tag_ids"] = new object[] { new object[] { 6, 0, tagIds.ToArray() } };

            var payload = ExecKw("project.task", "create", new object[] { vals });

            try
            {
                return await RpcAsync<int>(payload);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("MissingError") && ex.Message.Contains("project.project"))
            {
                throw new InvalidOperationException(
                    $"Cannot create task: project_id {projectId} not found/visible for uid={_uid}. " +
                    $"Fix project visibility/permissions/company or resolve by name.", ex);
            }
        }

        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            EnsureLoggedIn();
            var payload = ExecKw("res.users", "search",
                new object[] { new object[] { new object[] { "login", "=", email } } },
                new Dictionary<string, object> { ["limit"] = 1 });

            var ids = await RpcAsync<int[]>(payload);
            return ids.Length > 0 ? ids[0] : null;
        }

        // ---------------- extras you already had (optional to keep) ----------------

        public async Task<int[]> CreateTasksAsync(IEnumerable<OdooTaskSpec> tasks)
        {
            EnsureLoggedIn();

            var valsList = tasks?.Select(t =>
            {
                var vals = new Dictionary<string, object?>
                {
                    ["name"] = t.Title ?? throw new ArgumentNullException(nameof(t.Title)),
                    ["project_id"] = t.ProjectId
                };
                if (!string.IsNullOrWhiteSpace(t.Notes)) vals["description"] = t.Notes;
               
                //if (t.TagIds is not null) vals["tag_ids"] = new object[] { new object[] { 6, 0, t.TagIds.ToArray() } };
                return (object)vals;
            }).ToArray() ?? throw new ArgumentNullException(nameof(tasks));

            var payload = ExecKw("project.task", "create", new object[] { valsList });
            return await RpcAsync<int[]>(payload);
        }

        public async Task<int?> FindProjectIdByNameAsync(string projectName)
        {
            EnsureLoggedIn();
            var payload = ExecKw("project.project", "search",
                new object[] { new object[] { new object[] { "name", "=", projectName } } },
                new Dictionary<string, object> { ["limit"] = 1 });
            var ids = await RpcAsync<int[]>(payload);
            return ids.Length > 0 ? ids[0] : null;
        }

        public async Task<bool> ProjectExistsAsync(int projectId)
        {
            EnsureLoggedIn();
            var payload = ExecKw("project.project", "search_count",
                new object[] { new object[] { new object[] { "id", "=", projectId } } });
            var count = await RpcAsync<int>(payload);
            return count > 0;
        }

        // ---------------- internals ----------------

        private async Task LoginAsync()
        {
            EnsureConfig();

            var payload = new
            {
                jsonrpc = "2.0",
                method = "call",
                id = 1,
                @params = new { service = "common", method = "login", args = new object[] { _db!, _login!, _key! } }
            };

            var raw = await PostRawAsync(payload);
            using var doc = JsonDocument.Parse(raw);

            if (doc.RootElement.TryGetProperty("error", out var err))
                throw new InvalidOperationException($"Odoo RPC error (login): {err}");

            if (!doc.RootElement.TryGetProperty("result", out var result))
                throw new InvalidOperationException($"Odoo RPC: missing 'result' on login. Raw: {raw}");

            if (result.ValueKind == JsonValueKind.Number && result.TryGetInt32(out var uid) && uid > 0)
            { _uid = uid; return; }

            if (result.ValueKind == JsonValueKind.False)
                throw new UnauthorizedAccessException($"Odoo login failed: check DB/login/API key. api={_api}, db={_db}, login={_login}");

            throw new InvalidOperationException($"Unexpected login result: {result}. Raw: {raw}");
        }

        private void EnsureConfig()
        {
            if (string.IsNullOrWhiteSpace(_api) || !_api.EndsWith("/jsonrpc", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"_api must be the FULL jsonrpc URL. Got '{_api}'. Example: https://not4profit.online/jsonrpc");

            if (string.IsNullOrWhiteSpace(_db)) throw new InvalidOperationException("_db is empty");
            if (string.IsNullOrWhiteSpace(_login)) throw new InvalidOperationException("_login is empty");
            if (string.IsNullOrWhiteSpace(_key) || _key.Length < 20)
                throw new InvalidOperationException("API key is empty/too short.");
        }

        private void EnsureLoggedIn()
        {
            if (_uid <= 0) throw new InvalidOperationException("Call OdooGatewayconnection(...) first.");
            if (_api is null || _db is null || _login is null || _key is null)
                throw new InvalidOperationException("Gateway not initialized.");
        }

        private async Task<string> PostRawAsync(object payload)
        {
            var req = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(_api!, req);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }

        private async Task<T> RpcAsync<T>(object payload)
        {
            var raw = await PostRawAsync(payload);
            using var doc = JsonDocument.Parse(raw);

            if (doc.RootElement.TryGetProperty("error", out var err))
                throw new InvalidOperationException(err.ToString());

            if (!doc.RootElement.TryGetProperty("result", out var resultProp))
                throw new InvalidOperationException($"Odoo RPC: missing 'result'. Raw: {raw}");

            try
            {
                return JsonSerializer.Deserialize<T>(resultProp.GetRawText())!;
            }
            catch (JsonException je)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize Odoo result to {typeof(T).Name}. Raw result: {resultProp}", je);
            }
        }

        private object ExecKw(string model, string method, object[] positionalArgs, Dictionary<string, object>? kwargs = null)
            => new
            {
                jsonrpc = "2.0",
                method = "call",
                id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                @params = new
                {
                    service = "object",
                    method = "execute_kw",
                    args = new object[] { _db!, _uid, _key!, model, method, positionalArgs, kwargs ?? new Dictionary<string, object>() }
                }
            };
    }

   
}
