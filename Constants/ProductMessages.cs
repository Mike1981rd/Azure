namespace WebsiteBuilderAPI.Constants
{
    public static class ProductMessages
    {
        // Mensajes de éxito
        public static class Success
        {
            public const string ProductCreated = "Producto creado exitosamente";
            public const string ProductUpdated = "Producto actualizado correctamente";
            public const string ProductDeleted = "Producto eliminado exitosamente";
            public const string ProductsDeleted = "Productos eliminados exitosamente";
            public const string ProductActivated = "Producto activado correctamente";
            public const string ProductDeactivated = "Producto desactivado correctamente";
            public const string ImagesUpdated = "Imágenes del producto actualizadas exitosamente";
            public const string CollectionsAssigned = "Producto asignado a las colecciones seleccionadas";
            public const string VariantAdded = "Variante agregada exitosamente";
            public const string VariantUpdated = "Variante actualizada correctamente";
            public const string VariantDeleted = "Variante eliminada exitosamente";
            public const string InventoryUpdated = "Inventario actualizado correctamente";
            public const string ProductDuplicated = "Producto duplicado exitosamente";
            public const string ProductsImported = "Productos importados exitosamente";
            public const string ProductsExported = "Productos exportados correctamente";
        }

        // Mensajes de error - Validación
        public static class Validation
        {
            public const string NameRequired = "El nombre del producto es obligatorio";
            public const string NameTooLong = "El nombre del producto no puede exceder 200 caracteres";
            public const string NameAlreadyExists = "Ya existe un producto con este nombre";
            public const string InvalidPrice = "El precio debe ser mayor o igual a 0";
            public const string InvalidComparePrice = "El precio comparado debe ser mayor que el precio base";
            public const string InvalidStock = "El stock no puede ser negativo";
            public const string InvalidWeight = "El peso debe ser mayor que 0";
            public const string InvalidWeightUnit = "Unidad de peso inválida. Use: kg, g, lb, oz";
            public const string InvalidSKU = "El SKU contiene caracteres no válidos";
            public const string SKUAlreadyExists = "Ya existe un producto con este SKU";
            public const string BarcodeAlreadyExists = "Ya existe un producto con este código de barras";
            public const string InvalidBarcode = "El código de barras no es válido";
            public const string InvalidImageUrl = "Una o más URLs de imagen no son válidas";
            public const string TooManyImages = "No se pueden agregar más de 10 imágenes por producto";
            public const string InvalidProductType = "Tipo de producto no válido";
            public const string InvalidTags = "Los tags contienen caracteres no permitidos";
            public const string TooManyTags = "No se pueden agregar más de 20 tags por producto";
        }

        // Mensajes de error - Base de datos
        public static class Database
        {
            public const string SaveFailed = "Error al guardar el producto en la base de datos";
            public const string UpdateFailed = "Error al actualizar el producto";
            public const string DeleteFailed = "Error al eliminar el producto";
            public const string TransactionFailed = "Error en la transacción. Los cambios fueron revertidos";
            public const string ConnectionFailed = "No se pudo conectar con la base de datos";
            public const string TimeoutError = "La operación excedió el tiempo límite";
            public const string ConcurrencyError = "El producto fue modificado por otro usuario. Por favor, recargue e intente nuevamente";
        }

        // Mensajes de error - Negocio
        public static class Business
        {
            public const string ProductNotFound = "Producto no encontrado";
            public const string ProductsNotFound = "Uno o más productos no fueron encontrados";
            public const string UnauthorizedAccess = "No tiene permisos para realizar esta acción";
            public const string ProductHasOrders = "No se puede eliminar el producto porque tiene pedidos asociados";
            public const string ProductInCart = "No se puede eliminar el producto porque está en carritos activos";
            public const string InsufficientStock = "Stock insuficiente para completar la operación";
            public const string VariantRequired = "Este producto requiere seleccionar una variante";
            public const string NoVariantsAvailable = "No hay variantes disponibles para este producto";
            public const string CollectionNotFound = "Una o más colecciones no existen";
            public const string InvalidCompany = "Empresa no válida o no autorizada";
            public const string ProductInactive = "El producto está inactivo y no puede ser modificado";
            public const string CannotDeleteLastVariant = "No se puede eliminar la última variante del producto";
            public const string CannotDeactivateWithStock = "No se puede desactivar un producto con stock disponible";
        }

        // Mensajes de error - Archivo/Importación
        public static class FileOperation
        {
            public const string InvalidFileFormat = "Formato de archivo no válido. Use CSV o Excel";
            public const string FileTooLarge = "El archivo excede el tamaño máximo permitido (5MB)";
            public const string EmptyFile = "El archivo está vacío";
            public const string InvalidHeaders = "Los encabezados del archivo no son válidos";
            public const string ImportPartialSuccess = "Algunos productos no pudieron ser importados. Revise el log de errores";
            public const string ExportFailed = "Error al exportar los productos";
            public const string ImageUploadFailed = "Error al cargar una o más imágenes";
            public const string ImageSizeExceeded = "La imagen excede el tamaño máximo permitido (2MB)";
            public const string InvalidImageFormat = "Formato de imagen no válido. Use JPG, PNG o WEBP";
        }

        // Mensajes de advertencia
        public static class Warning
        {
            public const string LowStock = "Advertencia: Stock bajo para este producto";
            public const string NoImage = "El producto no tiene imagen principal";
            public const string NoDescription = "El producto no tiene descripción";
            public const string PriceNotSet = "El producto no tiene precio establecido";
            public const string DuplicateVariant = "Ya existe una variante con estos atributos";
            public const string CollectionLimit = "El producto ya está en el máximo de colecciones permitidas";
        }

        // Mensajes informativos
        public static class Info
        {
            public const string ProcessingRequest = "Procesando solicitud...";
            public const string LoadingProducts = "Cargando productos...";
            public const string GeneratingSKU = "Generando SKU automáticamente...";
            public const string CalculatingPrices = "Calculando precios...";
            public const string CheckingInventory = "Verificando inventario...";
            public const string SyncingVariants = "Sincronizando variantes...";
            public const string PreparingExport = "Preparando exportación...";
            public const string ValidatingData = "Validando datos...";
        }
    }
}