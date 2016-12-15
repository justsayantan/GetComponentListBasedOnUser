using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;

namespace GetComponent
{
    public partial class _Default : Page
    {
        #region Data Member
        public static List<UserDetails> users = new List<UserDetails>();
        public static string selectedUser = null;
        public static CoreServiceClient client = new CoreServiceClient();
        public static List<ComponentDetails> components = new List<ComponentDetails>();
        public static List<ComponentListBasedOnUser> componentListBasedOnUser = new List<ComponentListBasedOnUser>();
        #endregion

        /// <summary>
        /// Page Load Function. Load all the user and populate dropdown list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            LabelMessage.Text = "";
            if (!IsPostBack)
            {
                client = Utility.CoreServiceSource;
                var filter = new UsersFilterData();
                IdentifiableObjectData[] UserList = client.GetSystemWideList(filter);
                PopulateDropDownList(UserList);
            }

        }

        #region Protected Method

        /// <summary>
        /// Selected Index changed method for dropdown list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void userddList_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedUser = userddList.SelectedItem.Value;
        }

        /// <summary>
        /// Download List Button Click method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DownloadComponentList_Click(object sender, EventArgs e)
        {
            if (selectedUser != null)
            {
                if (selectedUser != "All Users")
                {
                    string tcmId = (from user in users where user.Description == selectedUser.ToString() select user.UserId).FirstOrDefault().ToString();
                    SearchQueryData filter = new SearchQueryData();
                    filter.Author = new LinkToUserData() { IdRef = tcmId };
                    filter.ItemTypes = new[] { ItemType.Component };
                    XElement result = client.GetSearchResultsXml(filter);
                    if (result.FirstNode != null)
                    {
                        ParseResult(result);
                    }
                    else
                    {
                        LabelMessage.Text = "No item created by " + selectedUser;
                    }
                }
                else
                {
                    foreach (var user in users)
                    {
                        SearchQueryData filter = new SearchQueryData();
                        filter.Author = new LinkToUserData() { IdRef = user.UserId };
                        filter.ItemTypes = new[] { ItemType.Component };
                        XElement result = client.GetSearchResultsXml(filter);
                        if (result.FirstNode != null)
                        {
                            ParseResultList(result, user.Description);
                        }
                    }
                    ExportCSVFileFromList(componentListBasedOnUser);
                }

            }

        }

        #endregion

        #region Private Method

        /// <summary>
        /// Populate the dropdownlist on page load
        /// </summary>
        /// <param name="userList"></param>
        private void PopulateDropDownList(IdentifiableObjectData[] userList)
        {
            List<string> userName = new List<string>();
            foreach (var item in userList)
            {
                if (!(item.Title.Contains("Administrator") || item.Title.Contains("MTSUser") || item.Title.Contains("SYSTEM") || item.Title.Contains("NETWORK")))
                {
                    UserDetails user = new UserDetails();
                    user.UserId = item.Id;
                    user.UserName = item.Title;
                    user.Description = ((TrusteeData)item).Description;
                    users.Add(user);
                }
            }
            userName = (from user in users select user.Description).ToList();
            userddList.DataSource = userName;
            userddList.DataBind();
            userddList.Items.Insert(0, new ListItem("--select User Name--", ""));
            userddList.Items.Insert(1, new ListItem("All Users", "All Users"));
        }

        /// <summary>
        /// Go through the list of the XMl and load the component data of each item for all the user
        /// </summary>
        /// <param name="result"></param>
        /// <param name="UserName"></param>
        private void ParseResultList(XElement result, string UserName)
        {
            ComponentListBasedOnUser _ComponentListBasedOnUser = new ComponentListBasedOnUser();
            foreach (var item in result.Elements())
            {
                var componentData = client.Read(item.Attribute("ID").Value, null) as ComponentData;
                ComponentDetails comDetail = new ComponentDetails();
                comDetail.ComponentId = componentData.Id;
                comDetail.ComponentLocation = componentData.LocationInfo.Path;
                comDetail.ComponentTitle = componentData.Title;
                _ComponentListBasedOnUser.ComponentList.Add(comDetail);
                
            }

            _ComponentListBasedOnUser.User = UserName;
            componentListBasedOnUser.Add(_ComponentListBasedOnUser);
        }

        /// <summary>
        /// Go through the list of the XMl and load the component data of each item for individual user
        /// </summary>
        /// <param name="result"></param>
        private void ParseResult(XElement result)
        {
            foreach (var item in result.Elements())
            {
                var componentData = client.Read(item.Attribute("ID").Value, null) as ComponentData;
                ComponentDetails comDetail = new ComponentDetails();
                comDetail.ComponentId = componentData.Id;
                comDetail.ComponentLocation = componentData.LocationInfo.Path;
                comDetail.ComponentTitle = componentData.Title;
                components.Add(comDetail);
            }
            ExportCSVFile(components);
        }

        /// <summary>
        /// Write the list of items into CSV file for all user
        /// </summary>
        /// <param name="componentListBasedOnUser"></param>
        private void ExportCSVFileFromList(List<ComponentListBasedOnUser> componentListBasedOnUser)
        {
            Response.Clear();
            Response.Buffer = true;
            string FileName = "attachment;filename=" + selectedUser + ".csv";
            Response.AddHeader("content-disposition", FileName);
            Response.Charset = "";
            Response.ContentType = "application/text";
            string strValue = string.Empty;
            foreach (var item in componentListBasedOnUser)
            {
                string strBody = null;
                string strUser = "Components Created By : " + item.User + Environment.NewLine;
                string strHeader = String.Format("{0},{1},{2}" + Environment.NewLine, "Component ID", "Component Title", "Component Location");
                foreach (var subitem in item.ComponentList)
                {
                    strBody = strBody + (String.Format("{0},{1},{2}" + Environment.NewLine, subitem.ComponentId, subitem.ComponentTitle, subitem.ComponentLocation));
                }
                strValue = strValue + strUser + strHeader + strBody + Environment.NewLine;
            }
            Response.Output.Write(strValue.ToString());
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// Write the list of items into CSV file for individual user
        /// </summary>
        /// <param name="components"></param>
        private void ExportCSVFile(List<ComponentDetails> components)
        {
            Response.Clear();
            Response.Buffer = true;
            string FileName = "attachment;filename=" + selectedUser + ".csv";
            Response.AddHeader("content-disposition", FileName);
            Response.Charset = "";
            Response.ContentType = "application/text";
            string strValue = string.Empty;
            string strBody = null;
            string strUser = "Components Created By : " + selectedUser + Environment.NewLine;
            string strHeader = String.Format("{0},{1},{2}" + Environment.NewLine, "Component ID", "Component Title", "Component Location");
            foreach (var item in components)
            {
                strBody = strBody + (String.Format("{0},{1},{2}" + Environment.NewLine, item.ComponentId, item.ComponentTitle, item.ComponentLocation));
                
            }
            strValue = strUser + strHeader + strBody;
            Response.Output.Write(strValue.ToString());
            Response.Flush();
            Response.End();

        }

        #endregion
    }
}