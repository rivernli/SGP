using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI.SGP.BLL.DataModels
{
    public class DataOptions
    {
        public const string DATAOPTIONS_VALUE_TRUE = "TRUE";
        public const string DATAOPTIONS_REQUIRED = "REQUIRED";
        public const string DATAOPTIONS_ALLOWZERO = "ALLOWZERO";
        public bool Required { get; set; }
        public bool AllowZero { get; set; }
        /// <summary>
        /// SCM Price Master字段是否可更新
        /// </summary>
        public bool UpdateKey { get; set; }
        /// <summary>
        /// SCM Price Master字段是否作为查询条件
        /// </summary>
        public bool ConditionKey { get; set; }

        //public DataOptions(string dataOptions)
        //{
        //    if (!String.IsNullOrWhiteSpace(dataOptions))
        //    {
        //        string[] options = dataOptions.Split(';');
        //        foreach(string option in options) 
        //        {
        //            string[] keyValue = option.Split(':');
        //            if (keyValue.Length == 2)
        //            {
        //                if (!String.IsNullOrWhiteSpace(keyValue[0]))
        //                {
        //                    switch (keyValue[0].Trim().ToUpper())
        //                    {
        //                        case "REQUIRED":
        //                            Required = Convert.ToString(keyValue[1].Trim().ToUpper()) == "TRUE";
        //                            break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataOptions"></param>
        /// <param name="pageType">SCM Price Master页面类型，用于判断Field是否可更新或作为判断条件</param>
        public DataOptions(string dataOptions, string pageType = "")
        {
            if (!String.IsNullOrWhiteSpace(dataOptions))
            {
                if (!string.IsNullOrEmpty(pageType))
                {
                    pageType = pageType.ToUpper();
                }
                string[] options = dataOptions.Split(';');
                foreach (string option in options)
                {
                    string[] keyValue = option.Split(':');
                    if (keyValue.Length == 2 && !String.IsNullOrWhiteSpace(keyValue[0]) && !String.IsNullOrWhiteSpace(keyValue[1]))
                    {
                        string key = keyValue[0].Trim().ToUpper();
                        string value = keyValue[1].Trim().ToUpper();
                        switch (key)
                            {
                            case DATAOPTIONS_REQUIRED:
                                Required = value == DATAOPTIONS_VALUE_TRUE;
                                break;
                            case DATAOPTIONS_ALLOWZERO:
                                AllowZero = value == DATAOPTIONS_VALUE_TRUE;
                                    break;
                                case "UPDATE":
                                    UpdateKey = Convert.ToString(keyValue[1].Trim().ToUpper()) == pageType;
                                    break;
                                case "CONDITION":
                                    ConditionKey = Convert.ToString(keyValue[1].Trim().ToUpper()) == pageType;
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
