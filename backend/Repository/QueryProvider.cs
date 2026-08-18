namespace backend.Repository;

public static class QueryProvider
{
    // =========================== PAGINATION HELPERS ===========================
    public static string WrapPaged(string sql) =>
        $"{sql} LIMIT @pageSize OFFSET ((@page - 1) * @pageSize)";

    public static string CountFrom(string sql) =>
        $"SELECT COUNT(*) FROM ({sql}) AS _count";

    // =========================== ITEMS ===========================
    public static string GetAllItems() =>
        "SELECT id, product_type_id AS ProductTypeId, name, description, category, quantity, unit, min_quantity AS MinQuantity, " +
        "donor, entry_date AS EntryDate, expiry_date AS ExpiryDate, created_at AS CreatedAt, " +
        "updated_at AS UpdatedAt FROM items ORDER BY id";

    public static string CountAllItems() =>
        "SELECT COUNT(*) FROM items";

    public static string GetItemById() =>
        "SELECT id, product_type_id AS ProductTypeId, name, description, category, quantity, unit, min_quantity AS MinQuantity, " +
        "donor, entry_date AS EntryDate, expiry_date AS ExpiryDate, created_at AS CreatedAt, " +
        "updated_at AS UpdatedAt FROM items WHERE id = @id";

    public static string InsertItem() =>
        "INSERT INTO items (name, product_type_id, description, category, quantity, unit, min_quantity, donor, entry_date, expiry_date) " +
        "VALUES (@name, @productTypeId, @description, @category, @quantity, @unit, @minQuantity, @donor, @entryDate, @expiryDate) " +
        "RETURNING id, product_type_id AS ProductTypeId, name, description, category, quantity, unit, min_quantity AS MinQuantity, " +
        "donor, entry_date AS EntryDate, expiry_date AS ExpiryDate, created_at AS CreatedAt, updated_at AS UpdatedAt";

    public static string UpdateItem() =>
        "UPDATE items SET name = COALESCE(@name, name), description = COALESCE(@description, description), " +
        "category = COALESCE(@category, category), quantity = COALESCE(@quantity, quantity), " +
        "unit = COALESCE(@unit, unit), min_quantity = COALESCE(@minQuantity, min_quantity), " +
        "donor = COALESCE(@donor, donor), entry_date = COALESCE(@entryDate, entry_date), " +
        "expiry_date = COALESCE(@expiryDate, expiry_date) WHERE id = @id " +
        "RETURNING id, product_type_id AS ProductTypeId, name, description, category, quantity, unit, min_quantity AS MinQuantity, " +
        "donor, entry_date AS EntryDate, expiry_date AS ExpiryDate, created_at AS CreatedAt, updated_at AS UpdatedAt";

    public static string DeleteItem() =>
        "DELETE FROM items WHERE id = @id";

    public static string AdjustQuantity() =>
        "UPDATE items SET quantity = quantity + @delta WHERE id = @id AND quantity + @delta >= 0 " +
        "RETURNING id, product_type_id AS ProductTypeId, name, description, category, quantity, unit, min_quantity AS MinQuantity, " +
        "donor, entry_date AS EntryDate, expiry_date AS ExpiryDate, created_at AS CreatedAt, updated_at AS UpdatedAt";

    public static string UpdateItemCategory() =>
        "UPDATE items SET category = @category WHERE id = @id";

    public static string UpdateItemEntryDate() =>
        "UPDATE items SET entry_date = @date WHERE id = @id";

    public static string UpdateItemExpiryDate() =>
        "UPDATE items SET expiry_date = @date WHERE id = @id";

    // =========================== PRODUCT TYPES ===========================
    public static string GetAllProductTypes() =>
        "SELECT pt.id, pt.name, pt.category AS Category, " +
        "COALESCE((SELECT COUNT(*) FROM items WHERE product_type_id = pt.id), 0) AS ItemCount, " +
        "COALESCE((SELECT SUM(quantity) FROM items WHERE product_type_id = pt.id), 0) AS TotalQuantity, " +
        "pt.created_at AS CreatedAt FROM product_types pt ORDER BY pt.name";

    public static string GetProductTypeById() =>
        "SELECT pt.id, pt.name, pt.category AS Category, " +
        "COALESCE((SELECT COUNT(*) FROM items WHERE product_type_id = pt.id), 0) AS ItemCount, " +
        "COALESCE((SELECT SUM(quantity) FROM items WHERE product_type_id = pt.id), 0) AS TotalQuantity, " +
        "pt.created_at AS CreatedAt FROM product_types pt WHERE pt.id = @id";

    public static string InsertProductType() =>
        "INSERT INTO product_types (name, category) VALUES (@name, @category) " +
        "RETURNING id, name, category AS Category, created_at AS CreatedAt";

    public static string UpdateProductType() =>
        "UPDATE product_types SET name = @name, category = @category WHERE id = @id " +
        "RETURNING id, name, category AS Category, created_at AS CreatedAt";

    public static string DeleteProductType() =>
        "DELETE FROM product_types WHERE id = @id";

    public static string DeleteItemsByProductType() =>
        "DELETE FROM items WHERE product_type_id = @productTypeId";

    public static string CountAllProductTypes() =>
        "SELECT COUNT(*) FROM product_types";

    public static string GetItemsByProductType() =>
        "SELECT id, product_type_id AS ProductTypeId, name, description, category, quantity, unit, min_quantity AS MinQuantity, " +
        "donor, entry_date AS EntryDate, expiry_date AS ExpiryDate, created_at AS CreatedAt, " +
        "updated_at AS UpdatedAt FROM items WHERE product_type_id = @productTypeId ORDER BY id";

    public static string SearchItems() =>
        "SELECT id, product_type_id AS ProductTypeId, name, description, category, quantity, unit, min_quantity AS MinQuantity, " +
        "donor, entry_date AS EntryDate, expiry_date AS ExpiryDate, created_at AS CreatedAt, updated_at AS UpdatedAt " +
        "FROM items WHERE quantity > 0 AND (@productTypeId = 0 OR product_type_id = @productTypeId) AND " +
        "(LOWER(name) LIKE LOWER(@q) OR LOWER(COALESCE(description, '')) LIKE LOWER(@q) OR LOWER(COALESCE(donor, '')) LIKE LOWER(@q)) " +
        "ORDER BY expiry_date IS NULL, expiry_date, id";

    // =========================== MOVEMENTS ===========================
    public static string InsertMovement() =>
        "INSERT INTO movements (item_id, item_name, type, quantity, description) " +
        "VALUES (@itemId, @itemName, @type::movement_type, @quantity, @description) " +
        "RETURNING id, item_id AS ItemId, item_name AS ItemName, type, quantity, date, description, source, created_at AS CreatedAt";

    public static string InsertHistoryMovement() =>
        "INSERT INTO movements (item_id, item_name, type, quantity, description, date, source) " +
        "VALUES (1, @itemName, @type::movement_type, @quantity, @description, @date, 'process') " +
        "RETURNING id, item_id AS ItemId, item_name AS ItemName, type, quantity, date, description, source, created_at AS CreatedAt";

    public static string GetAllMovements() =>
        "SELECT id, item_id AS ItemId, item_name AS ItemName, type, quantity, date, description, " +
        "source, created_at AS CreatedAt FROM movements ORDER BY date DESC";

    public static string CountAllMovements() =>
        "SELECT COUNT(*) FROM movements";

    public static string GetRecentMovements() =>
        "SELECT id, item_id AS ItemId, item_name AS ItemName, type, quantity, date, description, " +
        "source, created_at AS CreatedAt FROM movements ORDER BY date DESC LIMIT @limit";

    public static string BuildMovementsQuery(string? q, string? source, string? type, DateTime? from, DateTime? to)
    {
        var conditions = new List<string>();
        if (!string.IsNullOrWhiteSpace(q))
            conditions.Add("(LOWER(item_name) LIKE LOWER(@q) OR LOWER(COALESCE(description, '')) LIKE LOWER(@q))");
        if (!string.IsNullOrEmpty(source))
            conditions.Add("source = @source");
        if (type == "entry" || type == "exit")
            conditions.Add("type = @type::movement_type");
        if (from.HasValue)
            conditions.Add("date >= @from");
        if (to.HasValue)
            conditions.Add("date <= @to");

        var sql = "SELECT id, item_id AS ItemId, item_name AS ItemName, type, quantity, date, description, " +
                  "source, created_at AS CreatedAt FROM movements";
        if (conditions.Count > 0)
            sql += " WHERE " + string.Join(" AND ", conditions);
        return sql + " ORDER BY date DESC";
    }

    public static string GetMovementById() =>
        "SELECT id, item_id AS ItemId, item_name AS ItemName, type, quantity, date, description, source, " +
        "created_at AS CreatedAt FROM movements WHERE id = @id";

    public static string GetMovementGroupItems() =>
        "SELECT m2.id, m2.item_id AS ItemId, m2.item_name AS ItemName, m2.type, m2.quantity, m2.date, m2.description, " +
        "m2.source, m2.created_at AS CreatedAt " +
        "FROM movements m1 " +
        "JOIN movements m2 ON m2.description = m1.description AND m2.type = m1.type AND m2.date = m1.date " +
        "WHERE m1.id = @id ORDER BY m2.id";

    public static string GetMonthlyMovements() =>
        "SELECT EXTRACT(MONTH FROM date) AS month, type, SUM(quantity) AS total " +
        "FROM movements WHERE EXTRACT(YEAR FROM date) = EXTRACT(YEAR FROM CURRENT_DATE) " +
        "GROUP BY month, type ORDER BY month";

    public static string GetTotalEntries() =>
        "SELECT COALESCE(SUM(quantity), 0) FROM movements WHERE type = 'entry'";

    public static string GetTotalExits() =>
        "SELECT COALESCE(SUM(quantity), 0) FROM movements WHERE type = 'exit'";

    // =========================== CATEGORIES ===========================
    public static string GetAllCategories() =>
        "SELECT id, name, unit, created_at AS CreatedAt FROM categories ORDER BY id";

    public static string InsertCategory() =>
        "INSERT INTO categories (name) VALUES (@name) RETURNING id, name, unit, created_at AS CreatedAt";

    public static string GetCategoryById() =>
        "SELECT id, name, unit, created_at AS CreatedAt FROM categories WHERE id = @id";

    public static string UpdateCategoryById() =>
        "UPDATE categories SET name = @name WHERE id = @id RETURNING id, name, unit, created_at AS CreatedAt";

    public static string ClearCategoryFromProductTypes() =>
        "UPDATE product_types SET category = '' WHERE category = @name";

    public static string ClearCategoryFromItems() =>
        "UPDATE items SET category = '' WHERE category = @name";

    public static string RenameCategoryInProductTypes() =>
        "UPDATE product_types SET category = @newName WHERE category = @oldName";

    public static string RenameCategoryInItems() =>
        "UPDATE items SET category = @newName WHERE category = @oldName";

    public static string DeleteCategoryByName() =>
        "DELETE FROM categories WHERE name = @name";

    // =========================== NOTIFICATION EMAILS ===========================
    public static string GetAllEmails() =>
        "SELECT id, email, created_at AS CreatedAt FROM notification_emails ORDER BY id";

    public static string InsertEmail() =>
        "INSERT INTO notification_emails (email) VALUES (@email) RETURNING id, email, created_at AS CreatedAt";

    public static string DeleteEmail() =>
        "DELETE FROM notification_emails WHERE email = @email";

    // =========================== NOTIFICATION EVENTS ===========================
    public static string GetAllEvents() =>
        "SELECT id, event_key AS EventKey, enabled, label, created_at AS CreatedAt FROM notification_events ORDER BY id";

    public static string UpdateEventEnabled() =>
        "UPDATE notification_events SET enabled = @enabled WHERE event_key = @eventKey " +
        "RETURNING id, event_key AS EventKey, enabled, label, created_at AS CreatedAt";

    // =========================== DASHBOARD ===========================
    public static string DashboardTotalItems() =>
        "SELECT COUNT(*) FROM items";

    public static string DashboardTotalQuantity() =>
        "SELECT COALESCE(SUM(quantity), 0) FROM items";

    public static string DashboardLowStock() =>
        "SELECT COUNT(*) FROM items WHERE quantity <= min_quantity";

    public static string DashboardExpiringSoon() =>
        "SELECT COUNT(*) FROM items WHERE expiry_date IS NOT NULL " +
        "AND expiry_date BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '30 days'";

    // =========================== INBOUND PROCESSES ===========================
    public static string InsertInboundProcess() =>
        "INSERT INTO inbound_processes (name, description, start_date, end_date, type) " +
        "VALUES (@name, @description, @startDate, @endDate, @type) " +
        "RETURNING id, name, description, start_date AS StartDate, end_date AS EndDate, status, type, created_at AS CreatedAt";

    public static string GetAllInboundProcesses() =>
        "SELECT id, name, description, start_date AS StartDate, end_date AS EndDate, status, type, created_at AS CreatedAt " +
        "FROM inbound_processes ORDER BY created_at DESC";

    public static string CountAllInboundProcesses() =>
        "SELECT COUNT(*) FROM inbound_processes";

    public static string GetInboundProcessById() =>
        "SELECT id, name, description, start_date AS StartDate, end_date AS EndDate, status, type, created_at AS CreatedAt " +
        "FROM inbound_processes WHERE id = @id";

    public static string UpdateInboundProcessStatus() =>
        "UPDATE inbound_processes SET status = @status::process_status WHERE id = @id " +
        "RETURNING id, name, description, start_date AS StartDate, end_date AS EndDate, status, type, created_at AS CreatedAt";

    public static string InsertInboundItem() =>
        "INSERT INTO inbound_items (process_id, product_type_id, item_id, name, quantity, unit, expiry_date) " +
        "VALUES (@processId, @productTypeId, @itemId, @name, @quantity, @unit, @expiryDate) " +
        "RETURNING id, process_id AS ProcessId, product_type_id AS ProductTypeId, item_id AS ItemId, name, quantity, unit, expiry_date AS ExpiryDate, created_at AS CreatedAt";

    public static string GetInboundItemsByProcessId() =>
        "SELECT id, process_id AS ProcessId, product_type_id AS ProductTypeId, item_id AS ItemId, name, quantity, unit, expiry_date AS ExpiryDate, created_at AS CreatedAt " +
        "FROM inbound_items WHERE process_id = @processId ORDER BY created_at";

    public static string CountInboundItemsByProcessId() =>
        "SELECT COUNT(*) FROM inbound_items WHERE process_id = @processId";

    public static string DeleteInboundItem() =>
        "DELETE FROM inbound_items WHERE id = @id RETURNING id";

    public static string DeleteInboundItemsByProcessId() =>
        "DELETE FROM inbound_items WHERE process_id = @processId";

    public static string UpdateItemQuantity() =>
        "UPDATE items SET quantity = quantity + @delta WHERE id = @id";

    public static string InsertStockItemFromInbound() =>
        "INSERT INTO items (name, product_type_id, description, category, quantity, unit, donor, entry_date) " +
        "VALUES (@name, @productTypeId, '', 'Outros', @quantity, @unit, 'Contagem de entrada', CURRENT_DATE) RETURNING id";
}