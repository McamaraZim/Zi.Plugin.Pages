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
            }
        }
    }
}
