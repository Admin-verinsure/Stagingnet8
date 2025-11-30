using DealEngine.Domain.Entities.Abstracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Rendering;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealEngine.Domain.Entities
{
    public class EventsInfo : EntityBase, IAggregateRoot
    {
        public EventsInfo() : base(null) { }

        public EventsInfo(User createdBy) : base(createdBy)
        {
        }

        public EventsInfo(string month, string eventName, string location, string healthSafetyPlan, string eventType, ClientInformationSheet sheet, User user)
           : base(user)
        {
            Month = month;
            EventName = eventName;
            Location = location;
            HealthSafetyPlan = healthSafetyPlan;
            EventType = eventType;
            ClientInformationSheet = sheet;
        }

        public virtual string Month { get; set; }
        public virtual string EventName { get; set; }
        public virtual string Location { get; set; }
        public virtual  ClientInformationSheet ClientInformationSheet { get; set; }
       // public virtual IList<HealthSafetyPlan> healthSafetyPlan { get; set; }
        public virtual string HealthSafetyPlan { get; set; }
        public virtual string EventType { get; set; }

        //private IList<SelectListItem> GetSelectListOptions()
        //{
        //    return new List<SelectListItem>()
        //    {
        //        new SelectListItem
        //        {
        //            Text = "-- Select --", Value = "0"
        //        },
        //        new SelectListItem
        //        {
        //            Text = "Yes", Value = "1"
        //        },
        //        new SelectListItem
        //        { Text = "No", Value = "2" }
        //    };
        //}

        //private IList<SelectListItem> GetEventOptions()
        //{
        //    return new List<SelectListItem>()
        //    {
        //        new SelectListItem
        //        {
        //          Text = "-- Select --", Value = "0"},
        //        new SelectListItem
        //        {
        //          Text = "Event1", Value = "1"},
        //        new SelectListItem
        //        { 
        //          Text = "Event2", Value = "2" },
        //        new SelectListItem
        //        {
        //          Text = "Event4", Value = "3"},
        //        new SelectListItem
        //        { 
        //          Text = "Event5", Value = "4" 
        //        }

        //    };
        //}
    }
}

