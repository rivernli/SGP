using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SGP.DBUtility;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Reflection;


namespace BI.SGP.BLL.Models
{
    public class B2FComputedField
    {
        public B2FComputedField()
        {

        }

        public Dictionary<string, object> Data;
        public Dictionary<string, object> SetComputedValue()
        {
            TotalCost();
            return Data;
        }

        public void ASP()
        { 
        }
        public void ASPL()
        { 
        
        }
        public void SqInch()
        { 
        
        }
        public void CLsqin()
        { 
        
        
        }

        public void TargetVSActucal()
        { 
        
        
        }

        public void PanelUtilization()
        {
            
        }
        public void TotalCost()
        {
            if (Data.ContainsKey("18"))
            {
                Dictionary<string, object> cost = Data["18"] as Dictionary<string, object>;

                object objtotalcost = cost["FLOAT19"];
                object objvariablecost = cost["FLOAT20"];
                object objfixedcost = cost["FLOAT21"];
                double d = 0;
                double v = 0;
                double f = 0;
                if (objtotalcost is ArrayList)
                {
                    ArrayList total = objtotalcost as ArrayList;
                    ArrayList variable = objvariablecost as ArrayList;
                    ArrayList fixedcost = objfixedcost as ArrayList;

                    for (int i = 0; i < total.Count; i++)
                    {
                        
                        double.TryParse(variable[i].ToString(),out v);
                        double.TryParse(fixedcost[i].ToString(),out f);
                        d = v + f;
                        total[i] = d;

                    }

                    cost["FLOAT19"] = total;
                    Data["18"] = cost;
                }
                else
                {

                    double.TryParse(cost["FLOAT20"].ToString(), out v);
                    double.TryParse(cost["FLOAT21"].ToString(), out f);
                    d = v + f;
                    cost["FLOAT19"] = d;
                    Data["18"] = cost;
                }

            }
        }
        public void OP()
        {
            //get
            //{
            //    double i = 0;
            //    if (UnitPrice1 > 0 && TotalCost > 0)
            //    {
            //        i = (UnitPrice1 - TotalCost) / UnitPrice1;
            //    }
            //    return Math.Round(i * 100, 2); ;

            //}
            //set { }

        }
        public void MP()
        {
            //get
            //{
            //    double i = 0;
            //    if (UnitPrice1 > 0 && VariableCost > 0)
            //    {
            //        i = (UnitPrice1 - VariableCost) / UnitPrice1;
            //    }
            //    return Math.Round(i * 100, 2);

            //}

            //set { }
        }
    }
}