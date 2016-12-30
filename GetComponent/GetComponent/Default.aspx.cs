using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public static List<PublicationDetails> publications = new List<PublicationDetails>();
        public static string selectedUser = null;
        public static string selectedPublication = null;
        public static CoreServiceClient client = new CoreServiceClient();
        public static List<ItemDetails> items = new List<ItemDetails>();
        public static List<ItemListBasedOnUser> itemListBasedOnUser = new List<ItemListBasedOnUser>();
        #endregion

        /// <summary>
        /// Page Load Function. Load all the user and populate dropdown list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Load(object sender, EventArgs e)
        {
            LabelMessage.Text = "";
            if (!IsPostBack)
            {
                client = Utility.CoreServiceSource;
                var filter = new UsersFilterData();
                IdentifiableObjectData[] UserList = client.GetSystemWideList(filter);
                var filter1 = new PublicationsFilterData();
                IdentifiableObjectData[] PublicationList = client.GetSystemWideList(filter1);
                PopulateDropDownListUser(UserList);
                PopulateDropDownListPublication(PublicationList);
            }

        }
        

        #region Protected Method
        /// <summary>
        /// Selected Index changed method for Publication dropdown list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void publicationList_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedPublication = publicationddList.SelectedItem.Value;
        }

        /// <summary>
        /// Selected Index changed method for User dropdown list
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
            if (selectedUser != null && selectedPublication != null)
            {
                if (selectedUser != "All Users")
                {
                    string tcmId = (from user in users where user.Description == selectedUser.ToString() select user.UserId).FirstOrDefault().ToString();
                    string pubtcmId = (from pub in publications where pub.PublicationName == selectedPublication.ToString() select pub.PublicationId).FirstOrDefault().ToString();

                    SearchQueryData filter = new SearchQueryData();        
                    filter.Author = new LinkToUserData() { IdRef = tcmId };
                    filter.ItemTypes = new[] { ItemType.Component,ItemType.Page };
                    filter.SearchIn = new LinkToIdentifiableObjectData() { IdRef = pubtcmId };
                    filter.IncludeLocationInfoColumns = true;
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
                    string pubtcmId = (from pub in publications where pub.PublicationName == selectedPublication.ToString() select pub.PublicationId).FirstOrDefault().ToString();

                    foreach (var user in users)
                    {
                        SearchQueryData filter = new SearchQueryData();
                        filter.Author = new LinkToUserData() { IdRef = user.UserId };
                        filter.ItemTypes = new[] { ItemType.Component,ItemType.Page };
                        filter.SearchIn = new LinkToIdentifiableObjectData() { IdRef = pubtcmId };
                        filter.IncludeLocationInfoColumns = true;
                        XElement result = client.GetSearchResultsXml(filter);
                        if (result.FirstNode != null)
                        {
                            ParseResultList(result, user.Description);
                        }
                    }
                    ExportCSVFileFromList(itemListBasedOnUser);
                }
            }
            else
            {
                if (selectedUser == null)
                {
                    LabelMessage.Text = "Please select the User from dropdownlist";
                }
                else
                {
                    LabelMessage.Text = "Please select the Publication from dropdownlist";
                }
            }

        }

        #endregion

        #region Private Method
        
        /// <summary>
        /// Populate the dropdownlist on page load
        /// </summary>
        /// <param name="publicationList"></param>
        private void PopulateDropDownListPublication(IdentifiableObjectData[] publicationList)
        {
            List<string> publicationName = new List<string>();
            foreach (var item in publicationList)
            {
               
                    PublicationDetails pub = new PublicationDetails();
                    pub.PublicationId = item.Id;
                    pub.PublicationName = item.Title;
                    publications.Add(pub);
            }
            publicationName = (from publication in publications select publication.PublicationName).ToList();
            publicationddList.DataSource = publicationName;
            publicationddList.DataBind();
            publicationddList.Items.Insert(0, new ListItem("--select Publication--", ""));
        }


        /// <summary>
        /// Populate the dropdownlist on page load
        /// </summary>
        /// <param name="userList"></param>
        private void PopulateDropDownListUser(IdentifiableObjectData[] userList)
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
            string pubtcmId = (from pub in publications where pub.PublicationName == selectedPublication.ToString() select pub.PublicationId).FirstOrDefault().ToString();
            ItemListBasedOnUser _ItemListBasedOnUser = new ItemListBasedOnUser();
            foreach (var item in result.Elements())
            {
                    ItemDetails itemDetail = new ItemDetails();
                    itemDetail.ItemId = item.Attribute("ID").Value;
                    itemDetail.ItemLocation = item.Attribute("Path").Value.ToString();
                    itemDetail.ItemType = ((item.Attribute("Type").Value == "16") ? "Component" : "Page");
                    itemDetail.ItemTitle = item.Attribute("Title").Value;
                    _ItemListBasedOnUser.ComponentList.Add(itemDetail);

            }

            _ItemListBasedOnUser.User = UserName;
            itemListBasedOnUser.Add(_ItemListBasedOnUser);
        }

        /// <summary>
        /// Go through the list of the XMl and load the component data of each item for individual user
        /// </summary>
        /// <param name="result"></param>
        private void ParseResult(XElement result)
        {

            string pubtcmId = (from pub in publications where pub.PublicationName == selectedPublication.ToString() select pub.PublicationId).FirstOrDefault().ToString();
            
            foreach (var item in result.Elements())
            {
                    ItemDetails itemDetail = new ItemDetails();
                    itemDetail.ItemId = item.Attribute("ID").Value;
                    itemDetail.ItemLocation = String.Format(item.Attribute("Path").Value.ToString(), Encoding.Default);
                    itemDetail.ItemType = ((item.Attribute("Type").Value == "16")? "Component" : "Page");
                    itemDetail.ItemTitle = item.Attribute("Title").Value;
                    items.Add(itemDetail);

            }
            ExportCSVFile(items);
        }

        private bool PublicationMatch(string pubtcmId, string xAttributeValue)
        {
            Match match1 = Regex.Match(xAttributeValue, @"tcm:(\d*)-*");
            string key1 = match1.Groups[1].Value;
            Match match2 = Regex.Match(pubtcmId, @"tcm:\d*-(\d*)-*");
            string key2 = match2.Groups[1].Value;
            if(match1.Groups[1].Value == match2.Groups[1].Value)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Write the list of items into CSV file for all user
        /// </summary>
        /// <param name="componentListBasedOnUser"></param>
        private void ExportCSVFileFromList(List<ItemListBasedOnUser> itemListBasedOnUser)
        {
            Response.Clear();
            Response.Buffer = true;
            string FileName = "attachment;filename=" + selectedUser + ".csv";
            Response.AddHeader("content-disposition", FileName);
            Response.Charset = "";
            Response.ContentType = "application/text";
            string strValue = string.Empty;
            foreach (var item in itemListBasedOnUser)
            {
                string strBody = null;
                string strUser = "Components Created By : " + item.User + Environment.NewLine;
                string strHeader = String.Format("{0},{1},{2},{3}" + Environment.NewLine, "Item ID", "Item Title", "Item Type", "Item Location");
                foreach (var subitem in item.ComponentList)
                {
                    strBody = strBody + (String.Format("{0},{1},{2},{3}" + Environment.NewLine, subitem.ItemId, subitem.ItemTitle, subitem.ItemType, subitem.ItemLocation));
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
        private void ExportCSVFile(List<ItemDetails> items)
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
            string strHeader = String.Format("{0},{1},{2},{3}" + Environment.NewLine, "Item ID", "Item Title", "Item Type", "Item Location");
            foreach (var item in items)
            {
                strBody = strBody + (String.Format("{0},{1},{2},{3}" + Environment.NewLine, item.ItemId, item.ItemTitle, item.ItemType, item.ItemLocation));
                
            }
            strValue = strUser + strHeader + strBody;
            Response.Output.Write(strValue.ToString());
            Response.Flush();
            Response.End();

        }

        #endregion

        
    }
}