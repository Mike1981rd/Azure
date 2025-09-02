using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteBuilderAPI.Models.ThemeConfig
{
    /// <summary>
    /// Complete global theme configuration for a company website
    /// Stored as JSONB in PostgreSQL for flexibility and performance
    /// Replaces the monolithic 24,000-line JSON from v1
    /// </summary>
    [Table("GlobalThemeConfigs")]
    public class GlobalThemeConfig
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Company ID this configuration belongs to
        /// One company = one configuration (no multiple themes)
        /// </summary>
        [Required]
        public int CompanyId { get; set; }

        /// <summary>
        /// Reference to the company
        /// </summary>
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        /// <summary>
        /// Page appearance settings
        /// </summary>
        [Column(TypeName = "jsonb")]
        public AppearanceConfig Appearance { get; set; } = new AppearanceConfig();

        /// <summary>
        /// Typography settings for different text types
        /// </summary>
        [Column(TypeName = "jsonb")]
        public TypographyConfig Typography { get; set; } = new TypographyConfig();

        /// <summary>
        /// Color schemes (up to 5)
        /// </summary>
        [Column(TypeName = "jsonb")]
        public ColorSchemesConfig ColorSchemes { get; set; } = new ColorSchemesConfig();

        /// <summary>
        /// Product card display settings
        /// </summary>
        [Column(TypeName = "jsonb")]
        public ProductCardsConfig ProductCards { get; set; } = new ProductCardsConfig();

        /// <summary>
        /// Product badge settings
        /// </summary>
        [Column(TypeName = "jsonb")]
        public ProductBadgesConfig ProductBadges { get; set; } = new ProductBadgesConfig();

        /// <summary>
        /// Shopping cart configuration
        /// </summary>
        [Column(TypeName = "jsonb")]
        public CartConfig Cart { get; set; } = new CartConfig();

        /// <summary>
        /// Favicon settings
        /// </summary>
        [Column(TypeName = "jsonb")]
        public FaviconConfig Favicon { get; set; } = new FaviconConfig();

        /// <summary>
        /// Navigation and search settings
        /// </summary>
        [Column(TypeName = "jsonb")]
        public NavigationConfig Navigation { get; set; } = new NavigationConfig();

        /// <summary>
        /// Social media links
        /// </summary>
        [Column(TypeName = "jsonb")]
        public SocialMediaConfig SocialMedia { get; set; } = new SocialMediaConfig();

        /// <summary>
        /// Product variant swatches
        /// </summary>
        [Column(TypeName = "jsonb")]
        public SwatchesConfig Swatches { get; set; } = new SwatchesConfig();

        /// <summary>
        /// Version of the configuration schema
        /// Used for migrations and compatibility
        /// </summary>
        [Required]
        public string ConfigVersion { get; set; } = "2.0.0";

        /// <summary>
        /// When this configuration was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this configuration was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this configuration is published/active
        /// </summary>
        public bool IsPublished { get; set; } = false;

        /// <summary>
        /// When this configuration was published
        /// </summary>
        public DateTime? PublishedAt { get; set; }
    }

    #region Appearance Configuration
    public class AppearanceConfig
    {
        public int PageWidth { get; set; } = 2300;
        public int LateralPadding { get; set; } = 31;
        public string BorderRadius { get; set; } = "size-2";
    }
    #endregion

    #region Typography Configuration
    public class TypographyConfig
    {
        public FontConfig headings { get; set; } = new FontConfig();
        public FontConfig body { get; set; } = new FontConfig();
        public FontConfig menu { get; set; } = new FontConfig();
        public FontConfig productCardName { get; set; } = new FontConfig();
        public FontConfig buttons { get; set; } = new FontConfig();
    }

    public class FontConfig
    {
        public string fontFamily { get; set; } = "system-ui";
        public string fontWeight { get; set; } = "normal";
        public bool? useUppercase { get; set; }
        public int? fontSize { get; set; }
        public decimal? letterSpacing { get; set; }
    }
    #endregion

    #region Color Schemes Configuration
    public class ColorSchemesConfig
    {
        public string DefaultScheme { get; set; } = "scheme-1";
        public List<ColorScheme> Schemes { get; set; } = new List<ColorScheme>();
    }

    public class ColorScheme
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Text { get; set; } = "#000000";
        public string Background { get; set; } = "#FFFFFF";
        public string Foreground { get; set; } = "#F0F0F0";
        public string Border { get; set; } = "#CCCCCC";
        public string Link { get; set; } = "#0066CC";
        public string SolidButton { get; set; } = "#000000";
        public string SolidButtonText { get; set; } = "#FFFFFF";
        public string OutlineButton { get; set; } = "#000000";
        public string OutlineButtonText { get; set; } = "#000000";
        public string ImageOverlay { get; set; } = "rgba(0,0,0,0.3)";
    }
    #endregion

    #region Product Cards Configuration
    public class ProductCardsConfig
    {
        public ProductImageConfig Image { get; set; } = new ProductImageConfig();
        public ProductVisibilityConfig Visibility { get; set; } = new ProductVisibilityConfig();
        public ProductRatingConfig Rating { get; set; } = new ProductRatingConfig();
        public ProductPriceConfig Price { get; set; } = new ProductPriceConfig();
        public ProductEffectsConfig Effects { get; set; } = new ProductEffectsConfig();
        public ProductSwatchesConfig Swatches { get; set; } = new ProductSwatchesConfig();
        public ProductButtonsConfig Buttons { get; set; } = new ProductButtonsConfig();
    }

    public class ProductImageConfig
    {
        public string DefaultRatio { get; set; } = "square-1-1-fill";
    }

    public class ProductVisibilityConfig
    {
        public bool ShowVendor { get; set; }
        public bool ShowCurrencyCode { get; set; }
        public bool ShowColorCount { get; set; }
        public bool ColorizeCardBackground { get; set; }
        public bool DarkenImageBackground { get; set; }
    }

    public class ProductRatingConfig
    {
        public string ProductRating { get; set; } = "average-and-stars";
    }

    public class ProductPriceConfig
    {
        public string LabelSize { get; set; } = "large";
    }

    public class ProductEffectsConfig
    {
        public string HoverEffect { get; set; } = "none";
    }

    public class ProductSwatchesConfig
    {
        public string WhatToShow { get; set; } = "color-swatches";
        public string ShowOnDesktop { get; set; } = "on-hover";
        public string ShowOnMobile { get; set; } = "always";
        public bool HideForSingleColor { get; set; } = true;
    }

    public class ProductButtonsConfig
    {
        public bool QuickBuy { get; set; }
        public bool ShowQuickView { get; set; }
        public bool ShowAddToCart { get; set; }
        public string DesktopStyle { get; set; } = "labels";
        public bool ShowOnHover { get; set; } = true;
    }
    #endregion

    #region Product Badges Configuration
    public class ProductBadgesConfig
    {
        public BadgePositionConfig Position { get; set; } = new BadgePositionConfig();
        public BadgeConfig SoldOut { get; set; } = new BadgeConfig();
        public BadgeConfig Sale { get; set; } = new BadgeConfig();
        public BadgeConfig SaleByPrice { get; set; } = new BadgeConfig();
        public SaleHighlightConfig SaleHighlight { get; set; } = new SaleHighlightConfig();
        public BadgeConfig Custom1 { get; set; } = new BadgeConfig();
        public BadgeConfig Custom2 { get; set; } = new BadgeConfig();
        public BadgeConfig Custom3 { get; set; } = new BadgeConfig();
    }

    public class BadgePositionConfig
    {
        public string Desktop { get; set; } = "below-image";
    }

    public class BadgeConfig
    {
        public bool Enabled { get; set; }
        public string Background { get; set; } = "#000000";
        public string Text { get; set; } = "#FFFFFF";
        public string DisplayAs { get; set; } = "badge";
        public string TextContent { get; set; } = "";
        public string Tag { get; set; } = "";
    }

    public class SaleHighlightConfig
    {
        public bool Enabled { get; set; }
        public string TextColor { get; set; } = "#FF0000";
    }
    #endregion

    #region Cart Configuration
    public class CartConfig
    {
        public string DrawerType { get; set; } = "drawer-and-page";
        public bool ShowStickyCart { get; set; } = false;
        public CartStatusColorsConfig CartStatusColors { get; set; } = new CartStatusColorsConfig();
        public FreeShippingConfig FreeShipping { get; set; } = new FreeShippingConfig();
    }

    public class CartStatusColorsConfig
    {
        public string Background { get; set; } = "#F0FF2E";
        public string Text { get; set; } = "#383933";
    }

    public class FreeShippingConfig
    {
        public bool ShowProgress { get; set; } = true;
        public decimal Threshold { get; set; } = 50.00m;
        public string ProgressBarColor { get; set; } = "#28a745";
        public string SuccessMessage { get; set; } = "¡Envío gratis conseguido!";
        public string ProgressMessage { get; set; } = "Te faltan {amount} para envío gratis";
    }
    #endregion

    #region Favicon Configuration
    public class FaviconConfig
    {
        public bool CustomFavicon { get; set; } = true;
        public string FaviconUrl { get; set; } = "/favicon-custom.ico";
    }
    #endregion

    #region Navigation Configuration
    public class NavigationConfig
    {
        public SearchConfig Search { get; set; } = new SearchConfig();
        public BackToTopConfig BackToTop { get; set; } = new BackToTopConfig();
    }

    public class SearchConfig
    {
        public string ShowAs { get; set; } = "drawer-and-page";
    }

    public class BackToTopConfig
    {
        public bool ShowButton { get; set; } = true;
        public string Position { get; set; } = "bottom-left";
    }
    #endregion

    #region Social Media Configuration
    public class SocialMediaConfig
    {
        public string? Instagram { get; set; }
        public string? Facebook { get; set; }
        public string? Twitter { get; set; }
        public string? Youtube { get; set; }
        public string? Shopify { get; set; }
        public string? Pinterest { get; set; }
        public string? Tiktok { get; set; }
        public string? Tumblr { get; set; }
        public string? Snapchat { get; set; }
        public string? Linkedin { get; set; }
        public string? Vimeo { get; set; }
        public string? Flickr { get; set; }
        public string? Reddit { get; set; }
        public string? Email { get; set; }
        public string? Behance { get; set; }
        public string? Discord { get; set; }
        public string? Dublhub { get; set; }
        public string? Medium { get; set; }
        public string? Twitch { get; set; }
        public string? Whatsapp { get; set; }
        public string? Viber { get; set; }
        public string? Telegram { get; set; }
    }
    #endregion

    #region Swatches Configuration
    public class SwatchesConfig
    {
        // Primary swatch configuration
        public PrimarySwatchConfig? Primary { get; set; }
        
        // Secondary swatch configuration
        public SecondarySwatchConfig? Secondary { get; set; }
    }

    public class PrimarySwatchConfig
    {
        // Whether to show swatches
        public bool Enabled { get; set; }
        
        // Option name (e.g., "Color", "Size", etc.)
        public string? OptionName { get; set; }
        
        // Shape configuration for different contexts
        public string? ShapeForProductCards { get; set; } // Portrait, Round, Square, Landscape
        public int SizeForProductCards { get; set; } = 3; // 1-5
        
        public string? ShapeForProductPages { get; set; } // Round, Square, Portrait, Landscape
        public int SizeForProductPages { get; set; } = 4; // 1-5
        
        public string? ShapeForFilters { get; set; } // Square, Round, Portrait, Landscape
        public int SizeForFilters { get; set; } = 1; // 1-5
        
        // Custom colors and patterns (multiline text)
        public string? CustomColorsAndPatterns { get; set; }
    }

    public class SecondarySwatchConfig
    {
        // Option names for secondary swatches (multiline)
        public string? OptionNames { get; set; }
        
        // Shape configuration for different contexts
        public string? ShapeForProductPages { get; set; } // Square, Round, Portrait, Landscape
        public int SizeForProductPages { get; set; } = 4; // 1-5
        
        public string? ShapeForFilters { get; set; } // Square, Round, Portrait, Landscape
        public int SizeForFilters { get; set; } = 1; // 1-5
        
        // Custom colors and patterns (multiline text)
        public string? CustomColorsAndPatterns { get; set; }
    }
    #endregion
}