using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using Microsoft.Playwright;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DealEngine.Services.Impl
{
    public class PdfService : IPdfService
    {
        private static IPlaywright _playwright;
        private static IBrowser _browser;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public async Task<byte[]> GeneratePdfAsync(string html)
        {
            await EnsureBrowserAsync();

            var context = await _browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.SetContentAsync(html, new PageSetContentOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var pdf = await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin
                {
                    Top = "15mm",
                    Bottom = "15mm",
                    Left = "25mm",
                    Right = "25mm"
                },
                DisplayHeaderFooter = true,
                FooterTemplate =
                    "<div style='font-size:10px;width:100%;text-align:center;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></div>"
            });

            await context.CloseAsync();

            return pdf;
        }

        private async Task EnsureBrowserAsync()
        {
            if (_browser != null)
                return;

            await _lock.WaitAsync();
            try
            {
                if (_browser == null)
                {
                    _playwright = await Playwright.CreateAsync();

                    _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = true,
                        Args = new[]
                        {
                         "--no-sandbox",
                         "--disable-setuid-sandbox",
                         "--disable-dev-shm-usage"
                        }
                    });
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
