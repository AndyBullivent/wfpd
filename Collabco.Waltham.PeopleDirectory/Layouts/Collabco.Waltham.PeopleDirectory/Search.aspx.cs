using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.Office.Server;
using Microsoft.Office.Server.UserProfiles;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Collabco.Waltham.PeopleDirectory.Layouts.Collabco.Waltham.PeopleDirectory
{
   
    public partial class Search : LayoutsPageBase
    {
        private string GridViewSortExpression
        {
            get { return ViewState["GridViewSortExpression"] as string; }
            set 
            {
                ViewState["GridViewSortExpression"] = value;
                UserProfiles.DefaultView.Sort = string.Format("{0} {1}",value,GridViewSortDirection); 
            }
        }
        private string GridViewSortDirection
        {
            get { return ViewState["GridViewSortDirection"] as string; }
            set { ViewState["GridViewSortDirection"] = value; }
        }
        private string GridViewFilterExpression
        {
            get { return ViewState["GridViewFilterExpression"] as string; }
            set 
            {
                ViewState["GridViewFilterExpression"] = value;
                UserProfiles.DefaultView.RowFilter = value; 
            }
        }
        private List<UserProperty> UserProperties
        {
            get { return ViewState["UserProperties"] as List<UserProperty>; }
            set { ViewState["UserProperties"] = value; }
        }
        private DataTable UserProfiles
        {
            get { return ViewState["UserProfiles"] as DataTable; }
            set { ViewState["UserProfiles"] = value; }
            
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitialisePage();
                if (Request.QueryString["searchText"] != null)
                {
                    ApplyFilter(null, Request.QueryString["searchText"].ToString());
                }
                else
                    ApplyFilter(null, null); // do not want to show any data on page load
            }
            
            
        }

        protected void InitialisePage()
        {
            try
            {
                // get properties and profiles from cache
                UserProperties = UserProfileUtility.GetUserPropertiesFromPropertyBag().Where(p => !string.IsNullOrEmpty(p.DisplayName) && !p.ExcludeFromDirectorySearch).ToList<UserProperty>(); 
                UserProfiles = UserProfileUtility.GetUserProfilesFromCache();

                // set sorting and filtering initial values
                GridViewSortDirection = "ASC";
                GridViewSortExpression = UserProfiles != null ? UserProfiles.Columns[0].ColumnName : null;
                GridViewFilterExpression = null;
            }
            catch(Exception exp)
            {
                LogError(new SPException(
                    string.Format(" * An error occurred during initializing the page. "+
                    "Try reloading the page on a new browser window. If the problem persists then "+
                    "please contact support with quoting the below error message – '{0}'", exp.Message), exp));
            }
        }
        
        private void BindData()
        {
            try
            {
                LabelTableRowHeaderCell.Text = string.Format("Found {0} names - ordered by '{1}'", UserProfiles.DefaultView.Count, UserProfiles.DefaultView.Sort);
                GridView1.DataSource = UserProfiles;
                GridView1.DataBind();
                
            }
            catch (Exception exp)
            {
                LogError( new SPException(
                    string.Format(" * An error occurred during binding data to the grid view. " +
                    "Try reloading the page on a new browser window. If the problem persists then " +
                    "please contact support with quoting the below error message – '{0}'", exp.Message), exp));
            }
        }
        
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            foreach (TableCell cell in e.Row.Cells)
            {
                if(e.Row.RowType == DataControlRowType.DataRow)
                {
                    cell.Text = Server.HtmlDecode(cell.Text);
                }
            }
            
        }
        protected void GridView1_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (GridViewSortDirection.Equals("ASC"))
                GridViewSortDirection = "DESC";
            else
                GridViewSortDirection = "ASC";

            GridViewSortExpression = e.SortExpression;
            GridViewFilterExpression = GridViewFilterExpression;
            BindData();
            
        }
        protected int GetColumnIndex(string columnName)
        {
            int i = 0;
            foreach(DataControlField column in GridView1.Columns)
            {
                if(column.SortExpression == columnName)
                    return i;
                else
                    i++;
            }
            return i;
        }
        protected void GridView1_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                foreach (TableCell tc in e.Row.Cells)
                {
                    string columnName = ((LinkButton)tc.Controls[0]).Text;
                    UserProperty property = UserProperties.Where(p => p.DisplayName == columnName).FirstOrDefault();
                    if (property.UseInFilter)
                    {
                        TextBox tb = new TextBox();
                        tb.EnableViewState = tb.AutoPostBack = true;
                        tb.CssClass = "search_boxes";
                        tb.Attributes.Add("ColumnName", columnName);
                        tb.Attributes.Add("PlaceHolder", "Search...");
                        tb.ToolTip = "Search " + columnName;
                        tb.Text =property.SearchText;
                        tb.TextChanged += tb_TextChanged;
                        tc.Controls.Add(new LiteralControl("<br/>"));
                        tc.Controls.Add(tb);
                    }
                }
            }
        }
        void tb_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            ApplyFilter(tb.Attributes["ColumnName"], tb.Text);
            
        }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            ApplyFilter(null, null);
        }
        protected void ApplyFilter(string columnName, string filterValue)
        {
            try
            {
                
                LabelTableRowHeaderCell.Text = "";
                GridView1.EmptyDataText = "Sorry - there are no entries that match your requested search";

                //reset or Initial page load
                if (string.IsNullOrEmpty(columnName) && string.IsNullOrEmpty(filterValue))
                {
                    UserProperties.ForEach(p => p.SearchText = null);
                    GridViewFilterExpression = null;
                    GridView1.EmptyDataText = "";
                    GridView1.DataSource = UserProfiles.Clone(); // show empty grid
                    GridView1.DataBind();
                }
                else
                {
                    //query string search
                    if (string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(filterValue))
                    {
                        UserProperties.ForEach(p => p.SearchText = filterValue);
                        GridViewFilterExpression = CreateFilterExpression("OR");
                    }
                    else
                    {
                        //filter by column 
                        UserProperties.Where(p => p.DisplayName == columnName).FirstOrDefault().SearchText = filterValue;
                        GridViewFilterExpression = CreateFilterExpression("AND");
                    }
                    GridViewSortExpression = GridViewSortExpression; // not proud of this
                    BindData();
                }
                
                
            }
            catch(Exception exp)
            {
                LogError( new SPException(
                    string.Format(" * An error occurred during applying filter to the data on the gridview. " +
                    "Try reloading the page on a new browser window. If the problem persists then " +
                    "please contact support with quoting the below error message – '{0}'", exp.Message), exp));
            }
            
        }
        
        protected string CreateFilterExpression(string queryClause)
        {
            StringBuilder sb = new StringBuilder();
            foreach(UserProperty property in UserProperties.Where(p=> !string.IsNullOrEmpty(p.SearchText)))
            {
                if (sb.Length > 0)
                    sb.AppendFormat(" {0} ", queryClause);
                sb.AppendFormat("[{0}] like '%{1}%'", property.DisplayName, property.SearchText);
            }
            return sb.ToString();
        }
        protected void LogError(Exception exception)
        {
            PlaceHolderErrorMessage.Controls.Add(new LiteralControl(string.Format("<br/><font color=red><b>{0}</b></font><br/>",exception.Message)));
            PlaceHolderErrorMessage.Controls.Add(new LiteralControl(string.Format("Details: <font color=red>{0}</font><br/><br/>", exception.InnerException.ToString())));

        }
    }
}
