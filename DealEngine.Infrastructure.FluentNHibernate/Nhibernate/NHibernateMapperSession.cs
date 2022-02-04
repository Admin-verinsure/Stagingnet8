using DealEngine.Domain.Entities;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DealEngine.Infrastructure.FluentNHibernate
{
    public class NHibernateMapperSession<TEntity> : IMapperSession<TEntity> where TEntity : class
    {
        private ISession _session;
        private readonly ISessionFactory _sessionFactory;

        public NHibernateMapperSession(ISession session, ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
            _session = session;
        }

        public IQueryable<TEntity> FindAll()
        {
            return _session.Query<TEntity>();
        }

        public async Task<TEntity> GetByIdAsync(string id)
        {
            return await _session.GetAsync<TEntity>(id);
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            return await _session.GetAsync<TEntity>(id);
        }

        public async Task AddAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            if (!_session.IsOpen)
            {
                _session = _sessionFactory.OpenSession();
            }

            var transaction = _session.BeginTransaction();
            try
            {
                await _session.SaveOrUpdateAsync(entity);
                await transaction.CommitAsync();
            }
            catch(Exception ex)
            {
                //_logger.LogDebug(ex.Message);
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }

            transaction.Dispose();
        }

        public async Task RemoveAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            var transaction = _session.BeginTransaction();
            try
            {
                await _session.DeleteAsync(entity);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                //_logger.LogDebug(ex.Message);
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
            transaction.Dispose();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (!_session.IsOpen)
            {
                _session = _sessionFactory.OpenSession();
            }

            var transaction = _session.BeginTransaction();
            try
            {
                await _session.SaveOrUpdateAsync(entity);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                //_logger.LogDebug(ex.Message);
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }

            transaction.Dispose();
        }

        public Task<List<Object>> QueryHQLAsync(string query) 
        {            
            if (!_session.IsOpen)
            {
                _session = _sessionFactory.OpenSession();
            }

            List<Object> objects = new List<Object>();

            using (var tx = _session.BeginTransaction())
            {
                IQuery iQuery = _session.CreateQuery(query);
                foreach (var obj in iQuery.List<TEntity>())
                {
                    objects.Add(obj);
                }
            }
            return Task.FromResult(objects);
        }

        public Task<bool> GetStoredProcedure(Guid id, string BoatName)
        {

            //https://stackoverflow.com/questions/2568753/named-query-not-known-error-trying-to-call-a-stored-proc-using-fluent-nhibernate/2574647#2574647
            //https://stackoverflow.com/questions/16415877/fluent-nhibernate-to-query-stored-procedure-without-an-hbm-xml-mapping
            //https://mkyong.com/hibernate/hibernate-named-query-examples

            if (!_session.IsOpen)
            {
                _session = _sessionFactory.OpenSession();
            }

            List<Object> objects = new List<Object>();

            #region "GetNamedQuery" - No Parameters
            /*using (var tx = _session.BeginTransaction())
            {
                IQuery iQuery = _session.GetNamedQuery("select_test");
                iQuery.ExecuteUpdate();
            }*/

            // error: MappingException: Named query not known: select_test
            #endregion

            #region "GetNamedQuery" - With Parameter
            /*using (var tx = _session.BeginTransaction())
            {
                IQuery iQuery = _session.GetNamedQuery("insert_test4");
                iQuery.SetParameter(0, BoatName);
                iQuery.ExecuteUpdate();
            }*/

            // error: MappingException: Named query not known: insert_test4
            #endregion

            #region "CreateSQLQuery" - Param with colon
            /*var list = _session.CreateSQLQuery("EXEC insert_test3 :id, :boatname")
                .AddEntity(typeof(Boat))
                .SetParameter("id", id)
                .SetParameter("boatname", BoatName)
                .List<Boat>();*/

            // error: PostgresException: 42601: syntax error at or near "EXEC"
            #endregion

            #region "CreateSQLQuery" - SetGuid SetString
            /*var list = _session.CreateSQLQuery("exec insert_test3 :id, :boatname")
                .AddEntity(typeof(Boat))
                .SetGuid("id", id)
                .SetString("boatname", BoatName)
                .List<Boat>();*/

            // error: PostgresException: 42601: syntax error at or near "exec"
            #endregion

            #region "CreateSQLQuery" - With Parameters
            /*
            var list = _session.CreateSQLQuery("EXEC insert_test3 :parameter_1, :parameter_2")
                .AddEntity(typeof(Boat))
                .SetParameter("parameter_1", id)
                .SetParameter("parameter_2", BoatName)
                .List<Boat>();

            error: PostgresException: 42601: syntax error at or near "EXEC"
            */
            #endregion

            #region "CreateSQLQuery" Ordanary 
            // QueryParameterException: could not locate named parameter [a]
            //var list2 = _session.CreateSQLQuery("EXEC insert_test3 ?, ?")
            //    .AddEntity(typeof(Boat))
            //    .SetGuid(0, id)
            //    .SetString(1, BoatName)
            //    .List<Boat>();


            //var list = _session.CreateSQLQuery("CALL insert_test3(?1, ?2)")
            //    .AddEntity(typeof(Boat))
            //    .SetParameter(1, id)
            //    .SetParameter(2, BoatName)
            //    .List<object>(); //.AddEntity(typeof(Boat))
            //// "No positional parameters in query"
            ////https://stackoverflow.com/questions/20610221/illegalargumentexceptionno-positional-parameters-in-query

            //.SetParameter("parameter_1", id)
            //.SetParameter("parameter_2", BoatName)

            //var list = _session.CreateSQLQuery("CALL insert_test3() :parameter_1, :parameter_2")
            //.AddEntity(typeof(Boat))
            //.SetParameter("parameter_1", id)
            //.SetParameter("parameter_2", BoatName)
            //.List<Boat>();
            #endregion

            // The Procedure call that worked
            //var list = _session.CreateSQLQuery("CALL public.\"getallusernames\"()") // Changing from getallusernames() to public.\"getallusernames\"() didn't change behavior
            //    .AddEntity(typeof(User))
            //    .List<User>();

            ISQLQuery query = _session.CreateSQLQuery("SELECT public.\"GetUsernameOrg2\"()");
            //query.SetParameter("Username", username);
            //query.SetParameter("Username", username);

            //query.SetFirstResult(1);
            //string stringQuery = query.ToString();
            //query.AddEntity(entityName);
            //query.AddJoin(alias, path);
            //NHibernate.Type.IType type = new NHibernate.Type.IType();
            //query.AddScalar(columnAlias,type)

            //var result1 = query.List();  //      NullReferenceException: Object reference not set to an instance of an object.
            var result2 = query.List<string>();                   //      name:result2 | value:Count = 0 | type:System.Collections.Generic.IList<string> {System.Collections.Generic.List<string>}

            //IList<string> result3 = (IList<string>)query.List<(string,string)>(); // InvalidCastException: Unable to cast object of type 'System.Collections.Generic.List`1[System.ValueTuple`2[System.String,System.String]]' to type 'System.Collections.Generic.IList`1[System.String]'.
            //IList<string> result4 = (IList<string>)query.List();
            //IList<string> result5 = (IList<string>)query.List();


            //.List();                  NullReferenceException: Object reference not set to an instance of an object.
            //.List<(string,string)>(); NullReferenceException: Object reference not set to an instance of an object.
            //.List<object>();
            //.ToString();
            //.List<string>();

            // Get user.Org 
            // Now POC of joined data 


            return Task.FromResult(true);
        }
    }
}