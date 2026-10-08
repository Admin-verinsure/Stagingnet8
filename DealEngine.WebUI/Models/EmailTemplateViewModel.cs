using System;
using System.Collections.Generic;


namespace DealEngine.WebUI.Models
{
    public class EmailTemplateViewModel : BaseViewModel
    {
        public string Name { get; set; }

        public string Subject { get; set; }

        public string Type { get; set; }

        public string SystemEmailType { get; set; }

        public string Body { get; set; }
        public string Description { get; set; }
        public string InternalNotes { get; set; }
        public bool IsSystemTemplate { get; set; }

        public Guid BaseProgrammeID { get; set; }

        public Guid ClientProgrammeID { get; set; }

        public IEnumerable<UserViewModel> Recipents { get; set; }

        public Guid Recipent { get; set; }

    }


}