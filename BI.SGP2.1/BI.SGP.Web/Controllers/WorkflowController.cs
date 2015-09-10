using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using BI.SGP.BLL.WF;
using BI.SGP.BLL.Models;
using BI.SGP.BLL.DataModels;
using BI.SGP.BLL.Utils;
using SGP.DBUtility;

namespace BI.SGP.Web.Controllers
{
    public class WorkflowController : Controller
    {
        public ActionResult GetCurrentUserChildActivities()
        {
            string templateName = Request["templateName"];
            string entityId = Request["entityId"];
            int eid = 0;
            int.TryParse(entityId, out eid);
            WFTemplate wfTemplate = new WFTemplate(templateName, eid);

            string jsonData = "{\"activites\":[";
            if (wfTemplate.CurrentActivity != null && wfTemplate.CurrentActivity.CurrentUserChildActivities.Count > 0)
            {
                foreach (WFActivity wa in wfTemplate.CurrentActivity.CurrentUserChildActivities)
                {
                    jsonData += StringHelper.WFActivityToJSON(wa) + ",";
                }

                jsonData = jsonData.TrimEnd(',');
            }
            jsonData += "]}";
            return Content(jsonData);
        }

        //
        // GET: /Workflow/

        public ActionResult GetCurrentAndToActivity()
        {
            string templateName = Request["templateName"];
            int entityId = ParseHelper.Parse<int>(Request["entityId"]);
            int toId = ParseHelper.Parse<int>(Request["toId"]);

            WFTemplate template = new WFTemplate(templateName, entityId);

            string jsonData = "{\"currentActivity\":{";

            WFActivity act = null;

            if (template.CurrentActivity != null)
            {
                act = template.CurrentActivity;
                jsonData += String.Format("\"id\":{0},\"name\":{1},\"desc\":{2},\"sort\":{3},\"child\":[", act.ID, Newtonsoft.Json.JsonConvert.SerializeObject(act.Name), Newtonsoft.Json.JsonConvert.SerializeObject(act.Description), act.Sort);

                if (act.CurrentChildActivities.Count > 0)
                {
                    if (act.CurrentUserChildActivities.Count > 0)
                    {
                        foreach (WFActivity cuca in act.CurrentUserChildActivities)
                        {
                            jsonData += "{" + String.Format("\"id\":{0},\"name\":{1},\"desc\":{2},\"sort\":{3}", cuca.ID, Newtonsoft.Json.JsonConvert.SerializeObject(cuca.Name), Newtonsoft.Json.JsonConvert.SerializeObject(cuca.Description), cuca.Sort) + "},";
                        }
                        jsonData = jsonData.TrimEnd(',');
                    }
                    else
                    {
                        string desc = "";
                        foreach (WFActivity cca in act.CurrentChildActivities)
                        {
                            desc += String.Format("You have no permission to submit this, the current user for [{0}] are:Owner:{1}<br>", cca.Name, cca.OwnerUser);
                        }
                        jsonData += "{" + String.Format("\"id\":{0},\"name\":{1},\"desc\":{2},\"sort\":{3}", -999, Newtonsoft.Json.JsonConvert.SerializeObject(""), Newtonsoft.Json.JsonConvert.SerializeObject(desc), 1) + "}";
                    }
                }
                

                jsonData += "]";
            }
            jsonData += "},\"toActivity\":{";

            if (toId > 0)
            {
                act = template.Activities.Get(toId);
                if (act != null)
                {
                    jsonData += String.Format("\"id\":{0},\"name\":{1},\"desc\":{2},\"sort\":{3},\"child\":[", act.ID, Newtonsoft.Json.JsonConvert.SerializeObject(act.Name), Newtonsoft.Json.JsonConvert.SerializeObject(act.Description), act.Sort);
                    if (act.MatchChildActivities.Count > 0)
                    {
                        foreach (WFActivity cuca in act.MatchChildActivities)
                        {
                            jsonData += "{" + String.Format("\"id\":{0},\"name\":{1},\"desc\":{2},\"sort\":{3}", cuca.ID, Newtonsoft.Json.JsonConvert.SerializeObject(cuca.Name), Newtonsoft.Json.JsonConvert.SerializeObject(cuca.Description), cuca.Sort) + "},";
                        }
                        jsonData = jsonData.TrimEnd(',');
                    }
                    
                    jsonData += "]";
                }
            }

            jsonData += "}}";

            return Content(jsonData);
        }

        public ActionResult GetWizardDataForSupplierRfq(string number)
        {
            string templateName = Request["templateName"];
            string entityId = Request["entityId"];
            int eid = 0;
            int.TryParse(entityId, out eid);

            WFTemplate template = new WFTemplate(templateName, eid, number);

            string jsonData = "{\"activites\":[";

            foreach (WFActivity act in template.Activities)
            {
                jsonData += "{" + String.Format("\"id\":{0},\"name\":{1}, \"sort\":{2},\"activityType\":{3},\"activityDeac\":{4},\"child\":[", act.ID, Newtonsoft.Json.JsonConvert.SerializeObject(act.Name), act.Sort, Newtonsoft.Json.JsonConvert.SerializeObject(act.ActivityType), Newtonsoft.Json.JsonConvert.SerializeObject(act.Description));
                WFActivityCollection mcas = (eid == 0 || eid == -1) ? act.ChildActivities : act.MatchChildActivities;
                foreach (WFActivity cca in mcas)
                {
                    jsonData += "{" + String.Format("\"id\":{0},\"name\":{1}, \"sort\":{2},\"activityType\":{3},\"activityDeac\":{4}", cca.ID, Newtonsoft.Json.JsonConvert.SerializeObject(cca.Name), cca.Sort, Newtonsoft.Json.JsonConvert.SerializeObject(cca.ActivityType), Newtonsoft.Json.JsonConvert.SerializeObject(cca.Description)) + "},";
                }
                jsonData = jsonData.TrimEnd(',');
                jsonData += "]},";
            }

            jsonData = jsonData.TrimEnd(',');
            jsonData += "]";

            WFActivity curAct = null;
            if (eid == -1)
            {
                curAct = template.LastActivity;
            }
            else if (template.CurrentActivity != null)
            {
                curAct = template.CurrentActivity;
            }

            if (curAct != null)
            {
                jsonData += ",\"currentActivity\":{" + String.Format("\"id\":{0},\"name\":{1}, \"sort\":{2}, \"type\":{3}, \"child\":[", curAct.ID, Newtonsoft.Json.JsonConvert.SerializeObject(curAct.Name), curAct.Sort, Newtonsoft.Json.JsonConvert.SerializeObject(curAct.ActivityType));
                foreach (WFActivity cca in curAct.CurrentChildActivities)
                {
                    jsonData += "{" + String.Format("\"id\":{0},\"name\":{1}, \"sort\":{2},\"activityType\":{3},\"activityDeac\":{4}", cca.ID, Newtonsoft.Json.JsonConvert.SerializeObject(cca.Name), cca.Sort, Newtonsoft.Json.JsonConvert.SerializeObject(cca.ActivityType), Newtonsoft.Json.JsonConvert.SerializeObject(cca.Description)) + "},";
                }
                jsonData = jsonData.TrimEnd(',');
                jsonData += "]}";
            }

            jsonData += "}";

            return Content(jsonData);
        }

        public ActionResult GetWizardData()
        {
            string templateName = Request["templateName"];
            string entityId = Request["entityId"];
            int eid = 0;
            int.TryParse(entityId, out eid);

            WFTemplate template = new WFTemplate(templateName, eid);

            string jsonData = "{\"activites\":[";

            foreach (WFActivity act in template.Activities)
            {
                jsonData += "{" + String.Format("\"id\":{0},\"name\":{1}, \"sort\":{2},\"activityType\":{3},\"activityDeac\":{4},\"child\":[", act.ID, Newtonsoft.Json.JsonConvert.SerializeObject(act.Name), act.Sort, Newtonsoft.Json.JsonConvert.SerializeObject(act.ActivityType), Newtonsoft.Json.JsonConvert.SerializeObject(act.Description));
                WFActivityCollection mcas = (eid == 0 || eid == -1) ? act.ChildActivities : act.MatchChildActivities;
                foreach (WFActivity cca in mcas)
                {
                    jsonData += "{" + String.Format("\"id\":{0},\"name\":{1}, \"sort\":{2},\"activityType\":{3},\"activityDeac\":{4}", cca.ID, Newtonsoft.Json.JsonConvert.SerializeObject(cca.Name), cca.Sort, Newtonsoft.Json.JsonConvert.SerializeObject(cca.ActivityType), Newtonsoft.Json.JsonConvert.SerializeObject(cca.Description)) + "},";
                }
                jsonData = jsonData.TrimEnd(',');
                jsonData += "]},";
            }

            jsonData = jsonData.TrimEnd(',');
            jsonData += "]";

            WFActivity curAct = null;
            if (eid == -1)
            {
                curAct = template.LastActivity;
            }
            else if (template.CurrentActivity != null)
            {
                curAct = template.CurrentActivity;
            }

            if (curAct != null)
            {
                jsonData += ",\"currentActivity\":{" + String.Format("\"id\":{0},\"name\":{1}, \"sort\":{2}, \"type\":{3}, \"child\":[", curAct.ID, Newtonsoft.Json.JsonConvert.SerializeObject(curAct.Name), curAct.Sort, Newtonsoft.Json.JsonConvert.SerializeObject(curAct.ActivityType));
                foreach (WFActivity cca in curAct.CurrentChildActivities)
                {
                    jsonData += "{" + String.Format("\"id\":{0},\"name\":{1}, \"sort\":{2},\"activityType\":{3},\"activityDeac\":{4}", cca.ID, Newtonsoft.Json.JsonConvert.SerializeObject(cca.Name), cca.Sort, Newtonsoft.Json.JsonConvert.SerializeObject(cca.ActivityType), Newtonsoft.Json.JsonConvert.SerializeObject(cca.Description)) + "},";
                }
                jsonData = jsonData.TrimEnd(',');
                jsonData += "]}";
            }

            jsonData += "}";

            return Content(jsonData);
        }

        public ActionResult GetTimelineData()
        {
            string templateId = Request["templateId"];
            string entityId = Request["entityId"];
            int tid = 0;
            int eid = 0;
            int.TryParse(templateId, out tid);
            int.TryParse(entityId, out eid);
            string jsonData = "{}";
            if (eid > 0)
            {
                List<WFProcessLog> lstLog = WFHelper.GetProcessLog(tid, eid);
                jsonData = WFHelper.GetTimelineJSON(lstLog);
            }
            return Content(jsonData);
        }

        private string GetProcessPreview(int templateId)
        {
            List<WFActivity> activities = WFHelper.GetActivities(templateId);

            string headString = "<ul class='nav nav-tabs' id='myTab'>";
            string contentString = "<div class='tab-content'>";
            for (int i = 0; i < activities.Count; i++)
            {
                string[] users = GetUserString(activities[i]);
                string[] fields = GetFieldString(activities[i]);
                int curIndex = (i + 1);
                headString += "<li" + (curIndex == 1 ? " class='active'" : "") + "><a data-toggle='tab' href='#act" + curIndex + "'>" + " <span class='badge badge-info'>" + curIndex + "</span> " + activities[i].Name + "</a></li>";

                contentString += "<div id='act" + curIndex + "' class='tab-pane " + (curIndex == 1 ? "in active" : "") + "'>";

                contentString += "<div class='widget-box transparent'>";
                contentString += "<div class='widget-header widget-header-flat'><h4 class='lighter'><i class='icon-star orange'></i>";
                contentString += activities[i].Description;
                contentString += "</h4>";

                if (activities[i].ChildActivities.Count == 0)
                {
                    contentString += "<div class='widget-toolbar'><a target='_blank' href='" + Url.Content("~/Export/DownloadWFTemplate/" + activities[i].ID) + "'><h4 class='lighter'><i class='icon-download-alt'></i> Template</h4></a></div>";
                }

                contentString += "</div>";
                contentString += "<div class='widget-body'><div class='widget-main no-padding'>";

                if (activities[i].ChildActivities.Count > 0)
                {
                    contentString += GetChildPreview(activities[i].ChildActivities);
                }
                else
                {
                    contentString += "<table class='table table-bordered table-striped'><thead class='thin-border-bottom'><tr>";
                    contentString += "<th style='text-align:center;width:40%' colspan='2'><h4><i class='icon-group blue'></i> User Mapping</h4></th>";
                    contentString += "<th style='text-align:center;width:60%' colspan='2'><h4><i class='icon-list-alt blue'></i> Fields Mapping</h4></th>";
                    contentString += "</tr></thead>";
                    contentString += "<tbody id='act-content-" + activities[i].ID + "' class='act-content' acitivtyId='" + activities[i].ID + "'></tbody>";
                    contentString += "</table>";
                }

                contentString += "</div></div></div>";
                contentString += "</div>";
            }
            contentString += "</div>";
            headString += "</ul>";

            return headString + contentString;
        }

        public string GetChildPreview(List<WFActivity> activities)
        {
            string headString = "<ul class='nav nav-tabs' id='myChildTab'>";
            string contentString = "<div class='tab-content'>";
            for (int i = 0; i < activities.Count; i++)
            {
                string[] users = GetUserString(activities[i]);
                string[] fields = GetFieldString(activities[i]);
                int curIndex = (i + 1);
                headString += "<li" + (curIndex == 1 ? " class='active'" : "") + "><a data-toggle='tab' href='#actChild" + curIndex + "'> " + activities[i].Name + "</a></li>";

                contentString += "<div id='actChild" + curIndex + "' class='tab-pane " + (curIndex == 1 ? "in active" : "") + "'>";

                contentString += "<div class='widget-box transparent'>";
                contentString += "<div class='widget-header widget-header-flat'><h4 class='lighter'><i class='icon-star orange'></i>";
                contentString += activities[i].Description;
                contentString += "</h4>";
                contentString += "<div class='widget-toolbar'><a target='_blank' href='" + Url.Content("~/Export/DownloadWFTemplate/" + activities[i].ID) + "'><h4 class='lighter'><i class='icon-download-alt'></i> Template</h4></a></div>";

                contentString += "</div>";
                contentString += "<div class='widget-body'><div class='widget-main no-padding'>";
                contentString += "<table class='table table-bordered table-striped'><thead class='thin-border-bottom'><tr>";
                contentString += "<th style='text-align:center;width:40%' colspan='2'><h4><i class='icon-group blue'></i> User Mapping</h4></th>";
                contentString += "<th style='text-align:center;width:60%' colspan='2'><h4><i class='icon-list-alt blue'></i> Fields Mapping</h4></th>";
                contentString += "</tr></thead>";
                contentString += "<tbody id='act-content-" + activities[i].ID + "' class='act-content' acitivtyId='" + activities[i].ID + "'></tbody>";
                contentString += "</table></div></div></div>";
                contentString += "</div>";
            }
            contentString += "</div>";
            headString += "</ul>";

            return headString + contentString;
        }
        [MyAuthorize]
        public ActionResult ProcessPreview()
        {
            ViewBag.OutputString = GetProcessPreview(1);
            ViewBag.TemplateName = "DefaultWF";
            ViewBag.TemplateID = 1;
            return View();
        }
        [MyAuthorize]
        public ActionResult B2FProcessPreview()
        {
            ViewBag.OutputString = GetProcessPreview(2);
            ViewBag.TemplateName = "B2FQuotation";
            ViewBag.TemplateID = 2;
            return View("ProcessPreview");
        }

        public ActionResult GetActivityPreview(string ID)
        {
            bool isAdmin = AccessControl.CurrentLogonUser.IsAdmin();
            string contentString = "<tr>";
            contentString += "<td style='width:8%'><h4"+(isAdmin == true ? " style='cursor:pointer' onclick='showUserSetting(\"" + ID + "\", \"Owner\")'" : "") +">Owner</h4></td><td>{0}</td>";
            contentString += "<td style='width:9%'><h4" + (isAdmin == true ? " style='cursor:pointer' onclick='showFieldSetting(\"" + ID + "\", \"Required\")'" : "") + "> Required</h4></td><td>{1}</td>";
            contentString += "</tr><tr>";
            contentString += "<td><h4" + (isAdmin == true ? " style='cursor:pointer' onclick='showUserSetting(\"" + ID + "\", \"CC\")'" : "") + ">CC</h4></td><td>{2}</td>";
            contentString += "<td><h4" + (isAdmin == true ? " style='cursor:pointer' onclick='showFieldSetting(\"" + ID + "\", \"NotRequired\")'" : "") + ">Non Mandatory</h4></td><td>{3}</td>";
            contentString += "</tr>";

            string ownerUser = "";
            string ccUser = "";
            string isRequired = "";
            string notRequired = "";

            int activityId = 0;
            if (int.TryParse(ID, out activityId) && activityId > 0)
            {
                WFActivity act = new WFActivity(activityId);
                string[] users = GetUserString(act);
                string[] fields = GetFieldString(act);

                ownerUser = users[0];
                ccUser = users[1];
                isRequired = fields[0];
                notRequired = fields[1];
            }
            return Content(String.Format(contentString, ownerUser, isRequired, ccUser, notRequired));
        }

        private string[] GetUserString(WFActivity activity)
        {
            string ownerString = "<div class='clearfix'>";
            string ccString = "<div class='clearfix'>";

            Dictionary<string, WFUser> wfUsers = new Dictionary<string, WFUser>();

            activity.MergeUser(wfUsers, activity.GetStaticUsers());
            activity.MergeUser(wfUsers, activity.GetRoleUsers());

            activity.MergeUser(wfUsers, WFHelper.GetDelegateUsers(wfUsers));

            List<WFUser> lstPrevUser = activity.GetEntityPreviewUser();

            if (lstPrevUser != null)
            {
                foreach (WFUser u in lstPrevUser)
                {
                    if (u.IsApprover || u.IsKeyUser)
                    {
                        ownerString += GetUserString(u, "Specified");
                    }
                    else
                    {
                        ccString += GetUserString(u, "Specified");
                    }
                }
            }

            foreach (KeyValuePair<string, WFUser> kvUser in wfUsers)
            {
                if (kvUser.Value.IsApprover || kvUser.Value.IsKeyUser)
                {
                    ownerString += GetUserString(kvUser.Value, "Fixed");
                }
                else
                {
                    ccString += GetUserString(kvUser.Value, "Fixed");
                }													
            }

            ownerString += "</div>";
            ccString += "</div>";

            return new string[] { ownerString, ccString };
        }

        private string[] GetFieldString(WFActivity activity)
        {
            string requiredField = "<div class='clearfix'>";
            string notRequiredField = "<div class='clearfix'>";
            string fieldString = "";
            List<WFActivityField> lstField = activity.GetCheckFields();

            if (lstField != null)
            {
                foreach (WFActivityField f in lstField)
                {
                    fieldString = "<div class='itemdiv memberdiv'><div class='user'><i class='{0} icon-edit icon-3x'></i></div>";
                    fieldString += "<div class='body'>";
                    fieldString += "<div class='name'>" + f.DisplayName + "</div>";
                    fieldString += "<div><span class='label label-success label-sm'>" + f.Sort + "</span></div>";
                    fieldString += "</div></div>";

                    if (f.IsRequired)
                    {
                        requiredField += String.Format(fieldString, "blue"); ;
                    }
                    else
                    {
                        notRequiredField += String.Format(fieldString, "light-grey"); ;
                    }
                }
            }

            requiredField += "</div>";
            notRequiredField += "</div>";
            return new string[] { requiredField, notRequiredField };
        }

        private string GetUserString(WFUser user, string userType)
        {
            string userString = "";
            bool isKeyUser = (user.IsKeyUser || user.IsApprover);
            userString += "<div class='itemdiv memberdiv'><div class='user'><i class='" + (isKeyUser == true ? "blue" : "light-grey") + " icon-user icon-3x'></i></div>";
            userString += "<div class='body'>";
            userString += "<div class='name'>" + user.DisplayName + "</div>";
            userString += "<div><span class='label label-success label-sm'>" + userType + "</span></div>";
            userString += "</div></div>";
            return userString;
        }
    }
}
