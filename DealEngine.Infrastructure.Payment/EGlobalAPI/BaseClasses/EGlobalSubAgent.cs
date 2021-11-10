
using DealEngine.Domain.Entities;

namespace DealEngine.Infrastructure.Payment.EGlobalAPI.BaseClasses
{

    //default subagent name change
    //package product 
    public class EGlobalSubAgent
    {
        public string EGlobalDefaultSubAgent_SubCode { get; set; }
        public decimal EGlobalDefaultSubAgent_PercentComm { get; set; }
        public decimal EGlobalDefaultSubAgent_PercentCommNew { get; set; }
        public decimal EGlobalDefaultSubAgent_PercentCommRenew { get; set; }
        public decimal EGlobalDefaultSubAgent_SubAmount { get; set; }
        public decimal EGlobalDefaultSubAgent_SubAmountGST { get; set; }
        public int EGlobalDefaultSubAgent_RegistedGST { get; set; }
        public EGlobalSubAgent(SubAgent subAgent)
        {
            EGlobalDefaultSubAgent_SubCode = subAgent.SubCode;
            EGlobalDefaultSubAgent_PercentComm = subAgent.SubPercentCommn;
            EGlobalDefaultSubAgent_PercentCommNew = subAgent.SubPercentCommn;
            EGlobalDefaultSubAgent_PercentCommRenew = subAgent.SubPercentCommr;
            EGlobalDefaultSubAgent_RegistedGST = subAgent.SubGSTRegistered; 
        }

        public EBixSubAgent CalculateSubAgent(ClientInformationSheet clientInformationSheet, EBixPolicyRisk risk)
        {
            if (clientInformationSheet.IsRenewawl)
                EGlobalDefaultSubAgent_PercentComm = EGlobalDefaultSubAgent_PercentCommRenew;
            else
                EGlobalDefaultSubAgent_PercentComm = EGlobalDefaultSubAgent_PercentCommNew;

            EGlobalDefaultSubAgent_SubAmount = risk.BrokerAmountDue * (EGlobalDefaultSubAgent_PercentComm / 100m);
            EGlobalDefaultSubAgent_SubAmountGST = risk.GSTBrokerage * (EGlobalDefaultSubAgent_PercentComm / 100m);

            EBixSubAgent subAgent = new EBixSubAgent();
            subAgent.AmountCeded = EGlobalDefaultSubAgent_SubAmount;
            subAgent.CoverNumber = 0;
            subAgent.GSTFlag = EGlobalDefaultSubAgent_RegistedGST;
            if (subAgent.GSTFlag == -1)
            {
                subAgent.GSTCeded = EGlobalDefaultSubAgent_SubAmountGST;
            } else
            {
                subAgent.GSTCeded = 0M;
            }
            subAgent.Percent = EGlobalDefaultSubAgent_PercentComm;
            subAgent.SubAgent = EGlobalDefaultSubAgent_SubCode;
            subAgent.SubCover = risk.SubCover;
            subAgent.VerNo = 0;
            return subAgent;
        }
    }
}

