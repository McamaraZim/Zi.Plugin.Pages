namespace Nop.Plugin.Zimaltec.CustomPages.Constants;

public class Defaults
{
    public class Plugins
    {
        public const string KEY = "Plugins.Zimaltec";
        public class Zimaltec
        {
            public class CustomPages
            {
                public const string NAME = KEY + ".CustomPages";
                
                public static class AdminMenu
                {
                    public const string MANAGE_PAGES = "Zimaltec.CustomPages.Pages";
                }

                public class Permissions
                {
                    public const string MANAGE_NAME = "Manage Custom Pages";
                    public const string MANAGE_SYSTEM_NAME = NAME + ".Manage";
                    public const string MANAGE_CATEGORY = "Custom Pages";
                }

                public class Localization
                {
                    public const string KEY = Plugins.KEY + ".Localization";

                    public class Admin
                    {
                        public const string KEY = Localization.KEY + ".Admin";

                        public static class Menu
                        {
                            public const string KEY = Admin.KEY + ".Menu";
                            public const string ROOT = KEY + "Root";
                            public const string PAGES = KEY + "Pages";
                            public const string SECTIONS = KEY + "Sections";
                        }

                        public static class Pages
                        {
                            public const string KEY = Admin.KEY + ".Pages";
                            public const string LIST = KEY + "List";
                            public const string ADD_NEW = KEY + "AddNew";
                            public const string EDIT = KEY + "Edit";
                            public const string TITLE = KEY + "Title";
                            public const string SYSTEM_NAME = KEY + "SystemName";
                            public const string PUBLISHED = KEY + "Published";
                            public const string META_TITLE = KEY + "MetaTitle";
                            public const string META_DESCRIPTION = KEY + "MetaDescription";
                            public const string META_KEYWORDS = KEY + "MetaKeywords";
                            public const string BACK_TO_LIST  = KEY + "BackToList";
                            public const string TAB_SETTINGS  = KEY + "TabSettings";
                            public const string SEO_SECTION   = KEY + "SeoSection";
                            public const string SEO_SENAME_PLACEHOLDER = KEY + "SeoSeNamePlaceholder";
                            public const string SEO_SENAME_HINT        = KEY + "SeoSeNameHint";

                        }

                        public static class Sections
                        {
                            public const string KEY = Admin.KEY + ".Sections";
                            public const string LIST = KEY + "List";
                            public const string ADD_NEW = KEY + "AddNew";
                            public const string TOPIC = KEY + "Topic";
                            public const string NAME = KEY + "Name";
                            public const string DISPLAY_ORDER = KEY + "DisplayOrder";
                            public const string SYNC_BANNER = KEY + "SyncBanner";
                            public const string SYNC_NOW = KEY + "SyncNow";
                            public const string TAB_TITLE    = KEY + "TabTitle";
                            public const string LIST_TITLE   = KEY + "ListTitle";
                            public const string LOADING      = KEY + "Loading";
                            public const string BACK_TO_PAGE = KEY + "BackToPage";
                        }

                        public static class Fields
                        {
                            public const string KEY = Admin.KEY + ".Fields";
                            public const string TITLE = KEY + "Title";
                            public const string OBSOLETE = KEY + "Obsolete";
                            public const string TYPE = KEY + "Type";
                            public const string SETTINGS = KEY + "Settings";
                            public const string VALUE = KEY + "Value";
                            public const string DELETE_OBSOLETE = KEY + "DeleteObsolete";
                            public const string DELETE_OBSOLETE_CONFIRM = KEY + "DeleteObsolete.Confirm";
                            public const string NO_OBSOLETE = KEY + "NoObsolete";
                        }

                        public static class Common
                        {
                            public const string KEY = Admin.KEY + ".Common";
                            public const string SAVED = KEY + "Saved";
                            public const string ERROR = KEY + "Error";
                        }
                    }
                }
                
                public static partial class Views
                {
                    public const string ROOT = "~/Plugins/Zimaltec.CustomPages/Areas/Admin/Views/CustomPagesAdmin/";

                    public const string LIST_VIEW   = ROOT + "List.cshtml";
                    public const string CREATE_VIEW = ROOT + "Create.cshtml";
                    public const string EDIT_VIEW   = ROOT + "Edit.cshtml";
                    public const string ADD_SECTION_VIEW   = ROOT + "AddSection.cshtml";
                    public const string EDIT_SECTION_VIEW  = ROOT + "EditSection.cshtml";
                    public const string SECTION_LIST_VIEW  = ROOT + "_SectionsList.cshtml";
                    public const string SECTION_FIELDS_CREATE_VIEW = ROOT + "_SectionFieldsCreate.cshtml";
                    public const string CREATE_OR_UPDATE_VIEW = ROOT + "CreateOrUpdate.cshtml";
                    public const string CREATE_OR_UPDATE_SETTINGS_VIEW = ROOT + "CreateOrUpdate.Settings.cshtml";
                }
            }
        }
    }
}
