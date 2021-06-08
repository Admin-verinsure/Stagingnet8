//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using System.Collections.Generic;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DealEngine.WebUI.Models
{
    public class NTUcreateViewModel : BaseViewModel
    {
        public List<ClientInformationSheet> ClientInformationSheets { get; set; }
        public List<ClientProgramme> ClientProgrammes { get; set; }
        public string ProgrammeId { get; set; }
    }
}
