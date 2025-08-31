namespace WebsiteBuilderAPI.Models.Components
{
    /// <summary>
    /// Header configuration matching the frontend structure
    /// </summary>
    public class HeaderConfig
    {
        // Basic Settings
        public string ColorScheme { get; set; } = "1";
        public string Width { get; set; } = "page"; // screen, page, large, medium
        public int Height { get; set; } = 80; // Header height in pixels
        public string Layout { get; set; } = "drawer"; // drawer, logo-left-menu-center-inline, etc.
        public bool ShowSeparator { get; set; } = false;

        // Sticky Header Settings
        public StickyConfig Sticky { get; set; } = new StickyConfig();

        // Menu Settings
        public string MenuOpenOn { get; set; } = "hover"; // hover, click
        public int? MenuId { get; set; } = null; // ID of the NavigationMenu to use

        // Logo Settings
        public LogoConfig Logo { get; set; } = new LogoConfig();

        // Icon Settings
        public string IconStyle { get; set; } = "style2-outline"; // style1-solid, style1-outline, style2-solid, style2-outline
        
        // Cart Settings
        public CartConfig Cart { get; set; } = new CartConfig();

        // Theme Configuration (expandable section)
        public bool StickyCart { get; set; } = false;
        public string EdgeRounding { get; set; } = "size0"; // size0 through size8
        public string ShowAs1 { get; set; } = "drawer-and-page"; // drawer-and-page, page-only, none
        public string ShowAs2 { get; set; } = "drawer-and-page"; // drawer-and-page, page-only, none

        // Custom CSS
        public string CustomCss { get; set; } = "";
    }

    public class StickyConfig
    {
        public bool Enabled { get; set; } = false;
        public bool AlwaysShow { get; set; } = false;
        public bool MobileEnabled { get; set; } = false;
        public bool MobileAlwaysShow { get; set; } = false;
        public int InitialOpacity { get; set; } = 100;
    }

    public class LogoConfig
    {
        public string DesktopUrl { get; set; } = "/logo.png";
        public int Height { get; set; } = 190; // Desktop logo height in px
        public string MobileUrl { get; set; } = "";
        public int MobileHeight { get; set; } = 120; // Mobile logo height in px
        public string AltText { get; set; } = "Company Logo";
        public string Link { get; set; } = "/";
    }

    public class CartConfig
    {
        public string Style { get; set; } = "bag"; // bag, cart, basket
        public bool ShowCount { get; set; } = true;
        public string CountPosition { get; set; } = "top-right";
        public string CountBackground { get; set; } = "#000000";
        public string CountText { get; set; } = "#FFFFFF";
    }
}