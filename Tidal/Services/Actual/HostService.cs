using System;
using System.Collections.Generic;
using System.Linq;
using Tidal.Models;
using Tidal.Services.Abstract;

namespace Tidal.Services.Actual
{
    internal class HostService : IHostService
    {
        private readonly ISettingsService settingsService;
        private List<Host> hosts;
        private Guid activeId;

        public HostService(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            LoadHosts();
        }

        private void LoadHosts()
        {
            activeId = settingsService.ActiveHost;
            hosts = new List<Host>(settingsService.Hosts);

            foreach (var acct in hosts)
            {
                acct.MarkAsClean();
            }
        }

        public IReadOnlyList<Host> Hosts => hosts;

        public Host ActiveHost
        {
            get => GetHost(activeId);
            set
            {
                activeId = value.Id;
                foreach (var acct in hosts)
                    acct.Active = acct.Id == activeId;
            }
        }

        public Host GetHost(Guid id)
        {
            return hosts.SingleOrDefault(a => a.Id == id);
        }

        public int Count => hosts.Count;

        public void ReplaceAll(IEnumerable<Host> newAccounts)
        {
            if (Hosts == null || newAccounts == null)
                return;

            hosts.Clear();
            foreach (var c in newAccounts)
                hosts.Add(c.Clone());

            activeId = Hosts.Where(a => a.Active).Select(b => b.Id).FirstOrDefault();

            if (Count > 0 && activeId == Guid.Empty)
                throw new InvalidOperationException("Must define at least one account as active");
        }

        public void Save()
        {
            settingsService.Hosts = Hosts.ToList();
            settingsService.ActiveHost = activeId;
            settingsService.Save();
        }
    }
}
