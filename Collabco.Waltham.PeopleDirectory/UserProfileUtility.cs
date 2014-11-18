using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Server;
using Microsoft.Office.Server.UserProfiles;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace Collabco.Waltham.PeopleDirectory
{
    [Serializable]
    public class UserProperty
    {
        public int Sequence { get; set; }
        public string DisplayName { get; set; }
        public string PropertyName { get; set; }
        public bool UseInFilter { get; set; }
        public bool ExcludeFromDirectorySearch { get; set; }
        public string SearchText { get; set; }
    }

    public class UserProfileUtility
    {
        #region User Profile Private Members
        private static object _cacheLocker = new object();
        private static string _userProfileCacheKey = "AF38705D-2482-4122-B42F-1963401E19A3";
        private static DataTable LoadUserProfilesIntoCache()
        {
            DataTable userProfiles = GetUserProfiles();
            if(userProfiles != null)
                HttpContext.Current.Cache.Insert(_userProfileCacheKey, userProfiles, null, DateTime.Now.AddMinutes(10), TimeSpan.Zero);
            return userProfiles;
        }
        private static UserProfileManager GetDefaultUserProfileManager()
        {
            SPSite site = SPContext.Current.Site;
            SPServiceContext serviceContext = SPServiceContext.GetContext(site);
            UserProfileManager userProfileMgr = new UserProfileManager(serviceContext);
            return userProfileMgr;
        }
        private static bool ValueExists(UserProfile profile, UserProperty property)
        {
            try
            {
                var val = profile[property.PropertyName].Value;
                if (val != null)
                    return val.ToString().Length > 0;

                return false;
            }
            catch
            {
                return false;
            }
        }
        private static DataTable GetUserProfiles()
        {   
            List<UserProperty> userProperties = GetUserPropertiesFromPropertyBag();
            DataTable profileList = GetDataTableFromUserPropertyList(userProperties);
            var excludedProperties = userProperties.Where(p => p.ExcludeFromDirectorySearch).ToList<UserProperty>();
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                IEnumerator userProfiles = GetDefaultUserProfileManager().GetEnumerator();
                    while (userProfiles.MoveNext())
                    {
                        UserProfile each = (UserProfile)userProfiles.Current;

                        if (!excludedProperties.Any(p => ValueExists(each, p)))
                        {
                            DataRow newRow = CreateDataRow(profileList, userProperties, each);
                            profileList.Rows.Add(newRow);
                        }
                    }

            });
            return profileList;
        }
        private static DataRow CreateDataRow(DataTable profileList, List<UserProperty> userProperties, UserProfile each)
        {
            DataRow row = profileList.NewRow();

            foreach (DataColumn column in profileList.Columns)
            {
                var uProp = userProperties.Where(p => p.DisplayName == column.ColumnName).FirstOrDefault<UserProperty>();
                var value = each[uProp.PropertyName].Value;
                if (value != null)
                    row[uProp.DisplayName] = (IsValidEmail(value.ToString()) ? string.Format("<a href='mailto:{0}'>{0}</a>", value) : value);

            }
            return row;
        }
        #endregion

        #region Search Settings Private Members
        private static string _propertyBagKey = "6258BD72-625A-45D7-91C8-2EACD97D576A";
        private static List<UserProperty> _userProperties;
        
        private static string ObjectToString(object obj)
        {
            if (obj != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, obj);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            else
                return null;
        }

        private static object StringToObject(string base64String)
        {
            if (!string.IsNullOrEmpty(base64String))
            {
                byte[] bytes = Convert.FromBase64String(base64String);
                using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
                {
                    ms.Write(bytes, 0, bytes.Length);
                    ms.Position = 0;
                    return new BinaryFormatter().Deserialize(ms);
                }
            }
            else
                return null;
        }

        private static void AddIntoPropertyBag(object itemToAdd)
        {
            SPSite currentSite = SPContext.Current.Site;
            SPWeb rootWeb = currentSite.RootWeb;
            rootWeb.AllowUnsafeUpdates = true;
            if (!rootWeb.AllProperties.ContainsKey(_propertyBagKey))
            {
                rootWeb.AllProperties.Add(_propertyBagKey, null);
                rootWeb.Update();
            }
            rootWeb.AllProperties[_propertyBagKey] = ObjectToString(itemToAdd);
            rootWeb.Update();
            rootWeb.AllowUnsafeUpdates = false;
        }
        private static DataTable GetDataTableFromUserPropertyList(List<UserProperty> userPropertyList)
        {
            var dataTable = new DataTable("UserProperties");
            foreach (UserProperty property in userPropertyList)
            {
                if(!property.ExcludeFromDirectorySearch && !string.IsNullOrEmpty(property.DisplayName))
                    dataTable.Columns.Add(property.DisplayName, typeof(string));
            }
            return dataTable;
        }
        private static bool IsValidEmail(string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        }
        #endregion

        public static DataTable GetUserProfilesFromCache()
        {
            try
            {
                object cacheValue = HttpContext.Current.Cache.Get(_userProfileCacheKey);
                if (cacheValue is DataTable)
                    return (DataTable)cacheValue;
                else
                {
                    lock (_cacheLocker)
                    {
                        cacheValue = HttpContext.Current.Cache.Get(_userProfileCacheKey);
                        if (cacheValue is DataTable)
                            return (DataTable)cacheValue;
                        else
                            return LoadUserProfilesIntoCache();
                    }
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
        public static List<UserProperty> GetUserPropertiesFromPropertyBag()
        {
            try
            {
                if (_userProperties != null)
                    return _userProperties;
                SPSite currentSite = SPContext.Current.Site;
                SPWeb rootWeb = currentSite.RootWeb;

                if (rootWeb.AllProperties.ContainsKey(_propertyBagKey))
                    _userProperties = (List<UserProperty>)StringToObject((string)rootWeb.AllProperties[_propertyBagKey]);
                else
                {
                    _userProperties = new List<UserProperty>();
                    for (int i = 0; i < 15; i++)
                        _userProperties.Add(new UserProperty { Sequence = i + 1 });
                    AddIntoPropertyBag(_userProperties);
                }

                return _userProperties;
            }
            catch(Exception exp)
            {

                throw exp;
            }
        }
        public static bool SaveUserPropertiesToPropertyBag(List<UserProperty> propsToSave)
        {
            AddIntoPropertyBag(propsToSave);
            HttpContext.Current.Cache.Remove(_userProfileCacheKey);
            _userProperties = propsToSave;
            return true;
        }
        public static bool IsUserProfilePropertyValid(string propertyName)
        {
            try
            {
                UserProfile currentUserProfile = GetDefaultUserProfileManager().GetUserProfile(SPContext.Current.Web.CurrentUser.LoginName);
                object value = currentUserProfile[propertyName];
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static Dictionary<string,string> LoadPropertyListForCurrentUser()
        {
            Dictionary<string,string> list = new Dictionary<string,string>();
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                UserProfileManager upm = GetDefaultUserProfileManager();
                ProfileSubtypePropertyManager pspm = upm.DefaultProfileSubtypeProperties;
                UserProfile profile = upm.GetUserProfile(SPContext.Current.Web.CurrentUser.LoginName);

                foreach (ProfileSubtypeProperty prop in pspm.PropertiesWithSection)
                {
                    if (!prop.IsSection)
                    {
                        list.Add(prop.Name, (profile[prop.Name].Value != null ? profile[prop.Name].Value.ToString() : null));

                    }
                }

            });
            return list;
        }
    }
}
