CREATE DATABASE [Warehouse_25DH230022];
GO

USE [Warehouse_25DH230022];
GO

CREATE TABLE dbo.Products (
    ProductID   INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(100) NOT NULL,
    Category    NVARCHAR(100) NOT NULL,
    Price       DECIMAL(18,2) NOT NULL CHECK (Price > 0),
    Quantity    INT NOT NULL CHECK (Quantity >= 0)
);
GO

INSERT INTO dbo.Products (ProductName, Category, Price, Quantity)
VALUES
(N'Bàn phím Phát', N'Điện tử Phát',         350000, 12),
(N'Chuột Phát',    N'Điện tử Phát',         180000, 3),
(N'Tai nghe Phát', N'Điện tử Phát',         420000, 0),
(N'Bút bi Phát',   N'Văn phòng phẩm Phát',    5000, 20),
(N'Sổ tay Phát',   N'Văn phòng phẩm Phát',   25000, 5);
GO

SELECT COUNT(*) AS SoSanPham,
       COUNT(DISTINCT Category) AS SoDanhMuc
FROM dbo.Products;

USE [Warehouse_25DH230022];
GO

SELECT ProductName, COUNT(*) AS SoLan
FROM dbo.Products
GROUP BY ProductName
ORDER BY ProductName;

;WITH SanPhamTrung AS (
    SELECT ProductID,
           ROW_NUMBER() OVER (
               PARTITION BY ProductName
               ORDER BY ProductID
           ) AS SoThuTu
    FROM dbo.Products
)
DELETE FROM SanPhamTrung
WHERE SoThuTu > 1;

SELECT COUNT(*) AS SoSanPham,
       COUNT(DISTINCT Category) AS SoDanhMuc
FROM dbo.Products;