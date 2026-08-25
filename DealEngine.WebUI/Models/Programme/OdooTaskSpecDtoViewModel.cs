using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities;

namespace DealEngine.WebUI.Models.Programme
{

    //public class OdooTaskViewModel : BaseViewModel
    //{
    //    public ActivityBuilderVM Builder { get; set; }
    //    public ActivityAttachVM ActivityAttach { get; set; }
    //    public ActivityModal ActivityCreate { get; set; }        
    //    public Guid Id { get; set; }
    //};
    //public class OdooTaskSpecDtoViewModel : BaseViewModel
    //{
    //    public ActivityBuilderVM Builder { get; set; }
    //    public ActivityAttachVM ActivityAttach { get; set; }
    //    public ActivityModal ActivityCreate { get; set; }
    //    public Guid Id { get; set; }
    //};
    public sealed record OdooTaskSpecDtoViewModel(
       string Title,
       int ProjectId,
       Product product=null,
       string? notes = null
    );


}

