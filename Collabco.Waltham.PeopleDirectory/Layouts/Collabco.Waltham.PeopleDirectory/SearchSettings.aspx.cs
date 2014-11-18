using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Collabco.Waltham.PeopleDirectory.Layouts.Collabco.Waltham.PeopleDirectory
{
    public partial class SearchSettings : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindData();
                LoadPropertyList();
            }
        }
        protected void LoadPropertyList()
        {
            GridView3.DataSource = UserProfileUtility.LoadPropertyListForCurrentUser();
            GridView3.DataBind();
        }
        protected void GridView2_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            e.Row.Cells[1].Enabled = false;
            e.Row.Cells[e.Row.Cells.Count - 1].Visible = false;
        }
        private void BindData()
        {
            GridView2.DataSource = UserProfileUtility.GetUserPropertiesFromPropertyBag();
            GridView2.DataBind();
        }
        
        protected void buttonCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../settings.aspx");
        }

        protected void GridView2_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView2.EditIndex = e.NewEditIndex;
            BindData();
        }

        protected void GridView2_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            SetErrorText("");
            GridView2.EditIndex = -1;
            BindData();
        }
        protected void SetErrorText(string errorMessage)
        {
            LabelError.Text = errorMessage;
        }

        protected void GridView2_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int id = int.Parse(((TextBox)GridView2.Rows[e.RowIndex].Cells[1].Controls[0]).Text);
            string displayname = ((TextBox)GridView2.Rows[e.RowIndex].Cells[2].Controls[0]).Text;
            string propertyname = ((TextBox)GridView2.Rows[e.RowIndex].Cells[3].Controls[0]).Text;
            bool useinfilter = ((CheckBox)GridView2.Rows[e.RowIndex].Cells[4].Controls[0]).Checked;
            bool excludeFromDirectorySearch = ((CheckBox)GridView2.Rows[e.RowIndex].Cells[5].Controls[0]).Checked;
            if(!string.IsNullOrEmpty(propertyname))
                if (!IsValidUserProfileProperty(propertyname))
                {
                    SetErrorText(string.Format(" * PropertyName '{0}' not found in UserProfile Service", propertyname));
                    return;
                }
            if (string.IsNullOrEmpty(displayname) && !string.IsNullOrEmpty(propertyname))
            {
                SetErrorText(string.Format(" * Display name cannot be empty", displayname));
                return;
            }
            List<UserProperty> props = UserProfileUtility.GetUserPropertiesFromPropertyBag();
            var proptoUpdate = (from p in props
                               where p.Sequence == id
                               select p).FirstOrDefault<UserProperty>();
            proptoUpdate.DisplayName = displayname;
            proptoUpdate.PropertyName = propertyname;
            proptoUpdate.UseInFilter = useinfilter;
            proptoUpdate.ExcludeFromDirectorySearch = excludeFromDirectorySearch;
            UserProfileUtility.SaveUserPropertiesToPropertyBag(props);
            GridView2.EditIndex = -1;
            BindData();
            LabelError.Text = "";
        }
        protected bool IsValidUserProfileProperty(string propertyName)
        {
            return UserProfileUtility.IsUserProfilePropertyValid(propertyName);
        }
 
    }
}
