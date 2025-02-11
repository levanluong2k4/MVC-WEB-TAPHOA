create database webbanhang64131236
use webbanhang64131236



-- 1. QuanTri (No dependencies)
CREATE TABLE QuanTri (
    username varchar(50),
    Password varchar(50),
    Admin bit,
    CONSTRAINT pk_username PRIMARY KEY (username)
);


SELECT * FROM HoaDon h
JOIN DonHang d ON h.MaDH = d.MaDH
JOIN ChiTietDonHang ct ON d.MaDH = ct.MaDH
JOIN HangHoa hh ON ct.MaHH = hh.MaHH



-- 2. KhachHang (Depends on QuanTri)
CREATE TABLE KhachHang (
    MaKH varchar(10) not null,
    TenKH nvarchar(100) not null,
    DiaChi nvarchar(100) not null,
    Email varchar(50),
    username varchar(50),
    SDT_KH varchar(20) not null,
    CONSTRAINT pk_KhachHang PRIMARY KEY (MaKH),
    CONSTRAINT fk_username FOREIGN KEY (username) REFERENCES QuanTri(username) ON DELETE CASCADE
);

-- 3. NhanVien (Depends on QuanTri)
CREATE TABLE NhanVien (
    MaNV varchar(10) not null,
    username varchar(50),
    Ho nvarchar(30) not null,
    Ten nvarchar(10) not null,
    NgaySinh date not null,
    NgayLamViec date not null,
    DiaChi nvarchar(100) not null,
    DienThoai varchar(20) null,
    CONSTRAINT pk_NhanVien PRIMARY KEY (MaNV),
    CONSTRAINT fk_usernamenv FOREIGN KEY (username) REFERENCES QuanTri(username) ON DELETE CASCADE
);

-- 4. LoaiHang (No dependencies)
CREATE TABLE LoaiHang (
    MaLH varchar(10) not null,
    AnhLH nvarchar(100),
    TenLH nvarchar(20) not null,
    CONSTRAINT pk_LoaiHang PRIMARY KEY (MaLH)
);

-- 5. HangHoa (Depends on LoaiHang)
CREATE TABLE HangHoa (
    MaHH VARCHAR(10) NOT NULL,
    MaLH VARCHAR(10) NOT NULL,
    TenHH NVARCHAR(100) NOT NULL,
    AnhHH nvarchar(100),
    SoLuongHangTon INT NOT NULL CHECK (SoLuongHangTon >= 0),
    GiaBan MONEY NOT NULL,
    HSD DATETIME NOT NULL,
    NSX DATETIME NOT NULL,
    CONSTRAINT pk_HangHoa PRIMARY KEY (MaHH),
    CONSTRAINT fk_loaihanghoa FOREIGN KEY (MaLH) REFERENCES LoaiHang(MaLH) ON DELETE CASCADE
);

-- 6. GioHang (Depends on KhachHang)
CREATE TABLE GioHang (
    MaGioHang INT IDENTITY(1,1) PRIMARY KEY,
    MaKH VARCHAR(10) NOT NULL,
    NgayTao DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_GioHang_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
);

-- 7. ChiTietGioHang (Depends on GioHang and HangHoa)
CREATE TABLE ChiTietGioHang (
    MaGioHang INT,
    MaHH VARCHAR(10),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    CONSTRAINT pk_ChiTietGioHang PRIMARY KEY (MaGioHang, MaHH),
    CONSTRAINT fk_ChiTietGioHang_GioHang FOREIGN KEY (MaGioHang) REFERENCES GioHang(MaGioHang),
    CONSTRAINT fk_ChiTietGioHang_HangHoa FOREIGN KEY (MaHH) REFERENCES HangHoa(MaHH)
);

-- 8. Discounts (No dependencies)
CREATE TABLE Discounts (
    MaGiamGia VARCHAR(10) NOT NULL,          
    TenGiamGia NVARCHAR(100) NOT NULL,        
    LoaiGiamGia VARCHAR(20) NOT NULL,         
    GiaTriGiamGia DECIMAL(10, 2) NOT NULL,   
    NgayBatDau DATETIME NOT NULL,             
    NgayKetThuc DATETIME NOT NULL,          
    CONSTRAINT pk_Discounts PRIMARY KEY (MaGiamGia)
);

-- 9. DonHang (Depends on KhachHang and Discounts)
CREATE TABLE DonHang (
    MaDH INT IDENTITY PRIMARY KEY,
    NgayDat DATE NOT NULL DEFAULT GETDATE(),
    PhuongThucThanhToan NVARCHAR(20),
    MaGiamGia VARCHAR(10) NULL,
    TongTien DECIMAL(18, 2) NOT NULL CHECK (TongTien >= 0),
    TrangThai NVARCHAR(20) NOT NULL CHECK (TrangThai IN (N'Chờ xác nhận', N'Đã xác nhận', N'Đã hủy')),
    MaKH varchar(10) NOT NULL REFERENCES KhachHang(MaKH),
    CONSTRAINT fk_DonHang_Discounts FOREIGN KEY (MaGiamGia) REFERENCES Discounts(MaGiamGia) ON DELETE SET NULL
);
ALTER TABLE DonHang 
ADD TrangThaiGiaoHang NVARCHAR(50) DEFAULT N'Chưa giao hàng' 
CHECK (TrangThaiGiaoHang IN (N'Chưa giao hàng', N'Đang giao hàng', N'Giao hàng thành công', N'Đã nhận hàng'));

-- Thêm identity nếu chưa có




-- 10. ChiTietDonHang (Depends on DonHang and HangHoa)
CREATE TABLE ChiTietDonHang (
    MaDH INT NOT NULL REFERENCES DonHang(MaDH),
    MaHH varchar(10) NOT NULL REFERENCES HangHoa(MaHH),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18, 2) NOT NULL CHECK (DonGia >= 0),
    ThanhTien AS SoLuong * DonGia,
    PRIMARY KEY (MaDH, MaHH)
);

-- 12. ChiTietHoaDon (Depends on HoaDon and HangHoa)


-- Thêm ràng buộc ON DELETE cho khóa ngoại MaGioHang


-- Create table for delivery proof images
CREATE TABLE AnhGiaoHang (
    MaAnh INT IDENTITY(1,1) PRIMARY KEY,
    MaDH INT NOT NULL,
    DuongDanAnh NVARCHAR(255) NOT NULL,
    NgayTao DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_AnhGiaoHang_DonHang FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH)
);
SELECT * FROM GioHang WHERE MaKH = 'KH00007'
SELECT * FROM ChiTietGioHang WHERE MaGioHang =7

CREATE TABLE ThoiGianCapNhatDonHang (
    MaCapNhat INT PRIMARY KEY IDENTITY(1,1), -- Khóa chính với tự động tăng
    MaDH INT NOT NULL, -- Khóa ngoại liên kết đến bảng DonHang
    TrangThai NVARCHAR(255), -- Trạng thái (chuỗi ký tự Unicode)
    TrangThaiGiaoHang NVARCHAR(255), -- Trạng thái giao hàng (chuỗi ký tự Unicode)
    ThoiGianCapNhat DATETIME NOT NULL, -- Thời gian cập nhật (bắt buộc)

    -- Tạo khóa ngoại liên kết với bảng DonHang
    CONSTRAINT FK_ThoiGianCapNhatDonHang_DonHang FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH)
);

CREATE TABLE TraHang (
    MaTraHang INT IDENTITY(1,1) PRIMARY KEY,
    MaDH INT NOT NULL,
    NgayTraHang DATETIME DEFAULT GETDATE(),
    LyDoTraHang NVARCHAR(500) NOT NULL,
    NoiDungTraHang NVARCHAR(MAX), -- Detailed description of return
    TrangThaiTraHang NVARCHAR(50) DEFAULT N'Chờ xác nhận' 
        CHECK (TrangThaiTraHang IN (N'Chờ xác nhận', N'Đã xác nhận', N'Từ chối', N'Hoàn thành')),
    GhiChu NVARCHAR(500),
    CONSTRAINT FK_TraHang_DonHang FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH)
);

-- Create table for return images (AnhTraHang)
CREATE TABLE AnhTraHang (
    MaAnh INT IDENTITY(1,1) PRIMARY KEY,
    MaTraHang INT NOT NULL,
    DuongDanAnh NVARCHAR(255) NOT NULL,
    NgayThem DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_AnhTraHang_TraHang FOREIGN KEY (MaTraHang) REFERENCES TraHang(MaTraHang)
);

CREATE TABLE ChiTietTraHang (
    MaTraHang INT,
    MaHH VARCHAR(10),
    SoLuongTra INT NOT NULL CHECK (SoLuongTra > 0),
    LyDoTraChiTiet NVARCHAR(255),
    CONSTRAINT PK_ChiTietTraHang PRIMARY KEY (MaTraHang, MaHH),
    CONSTRAINT FK_ChiTietTraHang_TraHang FOREIGN KEY (MaTraHang) REFERENCES TraHang(MaTraHang),
    CONSTRAINT FK_ChiTietTraHang_HangHoa FOREIGN KEY (MaHH) REFERENCES HangHoa(MaHH)
);

CREATE TABLE DanhGiaSanPham (
    MaDanhGia INT IDENTITY(1,1) PRIMARY KEY,
    MaDH INT NOT NULL,
    MaHH VARCHAR(10) NOT NULL,
    SoSao INT NOT NULL CHECK (SoSao >= 1 AND SoSao <= 5),
    NoiDungDanhGia NVARCHAR(500),
    NgayDanhGia DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_DanhGia_DonHang FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH),
    CONSTRAINT FK_DanhGia_HangHoa FOREIGN KEY (MaHH) REFERENCES HangHoa(MaHH),
    CONSTRAINT FK_DanhGia_ChiTietDonHang FOREIGN KEY (MaDH, MaHH) REFERENCES ChiTietDonHang(MaDH, MaHH),
    -- Ensure one rating per product per order
    CONSTRAINT UQ_DanhGia_DonHang_HangHoa UNIQUE (MaDH, MaHH)
);







ALTER PROCEDURE XuLyDonHang
    @MaDH INT,
    @TrangThai NVARCHAR(20),
    @TrangThaiGiaoHang NVARCHAR(50) = NULL,
    @MaNV VARCHAR(10),
    @DuongDanAnh NVARCHAR(255) = NULL
AS
BEGIN
    BEGIN TRY
        -- Validate đầu vào
        IF NOT EXISTS (SELECT 1 FROM DonHang WHERE MaDH = @MaDH)
        BEGIN
            RAISERROR (N'Đơn hàng không tồn tại', 16, 1);
            RETURN;
        END

        IF @TrangThai NOT IN (N'Đã xác nhận', N'Đã hủy')
        BEGIN
            RAISERROR (N'Trạng thái không hợp lệ', 16, 1);
            RETURN;
        END

        IF @TrangThaiGiaoHang IS NOT NULL 
           AND @TrangThaiGiaoHang NOT IN (N'Chưa giao hàng', N'Đang giao hàng', N'Giao hàng thành công', N'Đã nhận hàng')
        BEGIN
            RAISERROR (N'Trạng thái giao hàng không hợp lệ', 16, 1);
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM NhanVien WHERE MaNV = @MaNV)
        BEGIN
            RAISERROR (N'Nhân viên không tồn tại', 16, 1);
            RETURN;
        END

        DECLARE @CurrentStatus NVARCHAR(20);
        SELECT @CurrentStatus = TrangThai FROM DonHang WHERE MaDH = @MaDH;

        BEGIN TRANSACTION
            -- Cập nhật trạng thái đơn hàng
            UPDATE DonHang 
            SET TrangThai = @TrangThai,
                TrangThaiGiaoHang = CASE 
                    WHEN @TrangThaiGiaoHang IS NOT NULL THEN @TrangThaiGiaoHang 
                    WHEN @TrangThai = N'Đã xác nhận' THEN N'Đang giao hàng'
                    ELSE TrangThaiGiaoHang 
                END
            WHERE MaDH = @MaDH;

            -- Nếu có ảnh giao hàng, thêm vào bảng AnhGiaoHang
            IF @DuongDanAnh IS NOT NULL AND @TrangThaiGiaoHang = N'Giao hàng thành công'
            BEGIN
                INSERT INTO AnhGiaoHang (MaDH, DuongDanAnh)
                VALUES (@MaDH, @DuongDanAnh);
            END

            -- Xử lý logic xác nhận đơn hàng như cũ
            IF @TrangThai = N'Đã xác nhận' AND @CurrentStatus != N'Đã xác nhận'
            BEGIN
                -- Kiểm tra số lượng tồn
                IF EXISTS (
                    SELECT 1
                    FROM ChiTietDonHang ct
                    JOIN HangHoa h ON ct.MaHH = h.MaHH
                    WHERE ct.MaDH = @MaDH AND ct.SoLuong > h.SoLuongHangTon
                )
                BEGIN
                    ROLLBACK;
                    RAISERROR (N'Một số sản phẩm không đủ số lượng tồn kho', 16, 1);
                    RETURN;
                END

                -- Tạo hóa đơn
                DECLARE @MaKH VARCHAR(10);
                SELECT @MaKH = MaKH FROM DonHang WHERE MaDH = @MaDH;

                INSERT INTO HoaDon (MaKH, MaNV, NgayGD, MaDH)
                VALUES (@MaKH, @MaNV, GETDATE(), @MaDH);

                -- Cập nhật số lượng tồn kho
                UPDATE HangHoa
                SET SoLuongHangTon = HangHoa.SoLuongHangTon - ChiTietDonHang.SoLuong
                FROM HangHoa
                JOIN ChiTietDonHang ON HangHoa.MaHH = ChiTietDonHang.MaHH
                WHERE ChiTietDonHang.MaDH = @MaDH;
            END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

CREATE TRIGGER trg_UpdateTongTien
ON DonHang
AFTER INSERT, UPDATE
AS
BEGIN
    UPDATE dh
    SET TongTien = 
        CASE 
            WHEN d.LoaiGiamGia = 'percent' THEN 
                (SELECT SUM(ct.SoLuong * ct.DonGia) FROM ChiTietDonHang ct WHERE ct.MaDH = dh.MaDH) * (1 - d.GiaTriGiamGia / 100.0)
            WHEN d.LoaiGiamGia = 'amount' THEN 
                (SELECT SUM(ct.SoLuong * ct.DonGia) FROM ChiTietDonHang ct WHERE ct.MaDH = dh.MaDH) - d.GiaTriGiamGia
            ELSE 
                (SELECT SUM(ct.SoLuong * ct.DonGia) FROM ChiTietDonHang ct WHERE ct.MaDH = dh.MaDH)
        END
    FROM DonHang dh
    LEFT JOIN Discounts d ON dh.MaGiamGia = d.MaGiamGia
    WHERE dh.MaDH IN (SELECT MaDH FROM Inserted);
END;
-- Create procedure for customers to confirm delivery
CREATE PROCEDURE XacNhanNhanHang
    @MaDH INT,
    @MaKH VARCHAR(10)
AS
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM DonHang WHERE MaDH = @MaDH AND MaKH = @MaKH)
        BEGIN
            RAISERROR (N'Đơn hàng không tồn tại hoặc không thuộc về khách hàng này', 16, 1);
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM DonHang 
                      WHERE MaDH = @MaDH 
                      AND TrangThaiGiaoHang = N'Giao hàng thành công')
        BEGIN
            RAISERROR (N'Đơn hàng chưa được giao thành công', 16, 1);
            RETURN;
        END

        UPDATE DonHang
        SET TrangThaiGiaoHang = N'Đã nhận hàng'
        WHERE MaDH = @MaDH AND MaKH = @MaKH;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;


CREATE PROCEDURE sp_HuyDonHang
    @MaDH INT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION
       
            IF NOT EXISTS (SELECT 1 FROM DonHang WHERE MaDH = @MaDH AND TrangThai = N'Chờ xác nhận')
            BEGIN
                RAISERROR (N'Chỉ có thể hủy đơn hàng ở trạng thái chờ xác nhận', 16, 1);
                RETURN;
            END;

            
            UPDATE DonHang 
            SET TrangThai = N'Đã hủy'
            WHERE MaDH = @MaDH;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;


CREATE TRIGGER trg_KiemTraSoLuongTon
ON ChiTietGioHang
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN HangHoa h ON i.MaHH = h.MaHH
        WHERE i.SoLuong > h.SoLuongHangTon
    )
    BEGIN
        RAISERROR ('Số lượng hàng trong kho không đủ', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;








CREATE TRIGGER trg_CapNhatSoLuongTon
ON DonHang
AFTER UPDATE
AS
BEGIN
   
    IF EXISTS (
        SELECT 1 
        FROM inserted i
        JOIN deleted d ON i.MaDH = d.MaDH
        WHERE i.TrangThai = N'Đã xác nhận' 
        AND d.TrangThai != N'Đã xác nhận'
    )
    BEGIN
      
        IF EXISTS (
            SELECT 1
            FROM ChiTietDonHang ct
            JOIN HangHoa hh ON ct.MaHH = hh.MaHH
            JOIN inserted i ON ct.MaDH = i.MaDH
            WHERE hh.SoLuongHangTon < ct.SoLuong
        )
        BEGIN
            RAISERROR (N'Không đủ số lượng hàng tồn để xác nhận đơn hàng', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

       
        UPDATE HangHoa
        SET SoLuongHangTon = HangHoa.SoLuongHangTon - ChiTietDonHang.SoLuong
        FROM HangHoa
        JOIN ChiTietDonHang ON HangHoa.MaHH = ChiTietDonHang.MaHH
        JOIN inserted ON ChiTietDonHang.MaDH = inserted.MaDH
        WHERE inserted.TrangThai = N'Đã xác nhận';
    END

   
    IF EXISTS (
        SELECT 1 
        FROM inserted i
        JOIN deleted d ON i.MaDH = d.MaDH
        WHERE i.TrangThai = N'Đã hủy' 
        AND d.TrangThai = N'Đã xác nhận'
    )
    BEGIN
        UPDATE HangHoa
        SET SoLuongHangTon = HangHoa.SoLuongHangTon + ChiTietDonHang.SoLuong
        FROM HangHoa
        JOIN ChiTietDonHang ON HangHoa.MaHH = ChiTietDonHang.MaHH
        JOIN inserted ON ChiTietDonHang.MaDH = inserted.MaDH
        WHERE inserted.TrangThai = N'Đã hủy';
    END
END;

DROP PROCEDURE IF EXISTS ThemVaoGioHang;

CREATE PROCEDURE ThemVaoGioHang
    @MaKH VARCHAR(10),
    @MaHH VARCHAR(10),
    @SoLuong INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
            DECLARE @MaGioHang INT;
            DECLARE @SoLuongTon INT;
            
            -- Check inventory
            SELECT @SoLuongTon = SoLuongHangTon 
            FROM HangHoa 
            WHERE MaHH = @MaHH;
            
            IF @SoLuongTon < @SoLuong
                THROW 51000, N'Số lượng hàng trong kho không đủ', 1;
            
            -- Get or create cart
            SELECT @MaGioHang = MaGioHang 
            FROM GioHang 
            WHERE MaKH = @MaKH;
            
            IF @MaGioHang IS NULL
            BEGIN
                INSERT INTO GioHang (MaKH, NgayTao)
                VALUES (@MaKH, GETDATE());
                
                SET @MaGioHang = SCOPE_IDENTITY();
            END
            
            -- Update or insert cart item
            MERGE ChiTietGioHang AS target
            USING (SELECT @MaGioHang AS MaGioHang, @MaHH AS MaHH, @SoLuong AS SoLuong) AS source
            ON (target.MaGioHang = source.MaGioHang AND target.MaHH = source.MaHH)
            WHEN MATCHED THEN
                UPDATE SET SoLuong = target.SoLuong + source.SoLuong
            WHEN NOT MATCHED THEN
                INSERT (MaGioHang, MaHH, SoLuong)
                VALUES (source.MaGioHang, source.MaHH, source.SoLuong);
                
        COMMIT TRANSACTION;
        
        SELECT 'Success' AS Status, N'Đã thêm vào giỏ hàng thành công' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 'Error' AS Status, ERROR_MESSAGE() AS Message;
    END CATCH;
END;



-- Check the constraint definition
SELECT * FROM sys.check_constraints 
WHERE name = 'CK__DonHang__TrangTh__07C12930'

INSERT INTO QuanTri (username, Password, Admin) VALUES
    ('admin01', 'password123', 1),
    ('user02', 'password456', 0),
    ('user03', 'password789', 0),
    ('admin04', 'password321', 1),
    ('user05', 'password654', 0),
    ('user06', 'password987', 0),
    ('admin07', 'password111', 1),
    ('user08', 'password222', 0),
    ('user09', 'password333', 0),
    ('admin10', 'password444', 1),
    ('user11', 'password555', 0),
    ('user12', 'password666', 0),
    ('admin13', 'password777', 1),
    ('user14', 'password888', 0),
    ('user15', 'password999', 0),
    ('admin16', 'password000', 1),
    ('user17', 'passwordaaa', 0),
    ('user18', 'passwordbbb', 0),
    ('user19', 'passwordccc', 0),
    ('admin20', 'passwordddd', 1);


	INSERT INTO KhachHang (MaKH, TenKH, DiaChi, SDT_KH, Email, username) VALUES
    ('KH00001', N'Nguyễn Nam An', N'123 Trường Chinh, Quận 1, TP.HCM', '0843376096', 'nguyen.naman@example.com', 'user02'),
    ('KH00002', N'Trần Trung Hiếu', N'22/153 Võ Nguyên Giáp, Nam Từ Liêm, TP.Hà Nội', '0765331442', 'tran.trunghieu@example.com', 'user03'),
    ('KH00003', N'Lê Văn Chiến', N'78/63, Quận 3, TP.Đà Nẵng', '0345678812', 'le.vanchien@example.com', 'user05'),
    ('KH00004', N'Phạm Hữu Phát', N'101 Nguyễn Thị Minh Khai, TP.Phan Rang Tháp Chàm', '0883336107', 'pham.huuphat@example.com', 'user06'),
    ('KH00005', N'Hoàng Tiến Dũng', N'55 Lê Duẩn, Quận 5, TP.Hải Phòng', '0744155162', 'hoang.tiendung@example.com', 'user08'),
    ('KH00006', N'Ngô Nhiên An', N'103 Phan Châu Trinh, TP.Quy Nhơn', '0678912349', 'ngo.nhienan@example.com', 'user09'),
    ('KH00007', N'Bùi Quyết Thắng', N'73/12 Phan Bội Châu, TP.Nha Trang', '0787123456', 'bui.quyetthang@example.com', 'user11'),
    ('KH00008', N'Dương Thị Hồng', N'23 Lê Lợi, TP.Huế', '0991234567', 'duong.thihong@example.com', 'user12'),
    ('KH00009', N'Lương Quốc Đại', N'114/56 Trường Chinh, TP.Phan Rang Tháp Chàm', '0912345678', 'luong.quocdai@example.com', 'user14'),
    ('KH00010', N'Võ Văn Giáp', N'113 Nguyễn Trãi, TP.Quy Nhơn', '0123456789', 'vo.vangiap@example.com', 'user15');

	select * from KhachHang
	INSERT INTO NhanVien (MaNV, username, Ho, Ten, NgaySinh, NgayLamViec, DiaChi, DienThoai) VALUES
    ('NV0001', 'admin01', N'Cao Tiến', N'Đạt', '1990-08-13', '2019-03-28', N'1 Lê Lợi, Quận 1, TP.HCM', '0937543210'),
    ('NV0002', 'admin04', N'Dương Trung', N'Quốc', '1990-01-10', '2019-03-28', N'2 Hai Bà Trưng, Quận Bình Thạnh, TP.HCM', '0946465213'),
    ('NV0003', 'admin07', N'Dương Quang', N'Tùng', '1992-01-13', '2020-01-15', N'2 Hai Bà Trưng, Quận Bình Thạnh, TP.HCM', '0914476234'),
    ('NV0004', 'admin10', N'Nguyễn Thị', N'Trúc', '1991-03-01', '2020-01-01', N'2 Hai Bà Trưng, Quận Bình Thạnh, TP.HCM', '0946465213'),
    ('NV0005', 'admin13', N'Giang Thị', N'Hương', '1991-02-23', '2020-01-15', N'3 Nguyễn Đình Chiểu, TP.Nha Trang', '0944763310'),
    ('NV0006', 'admin16', N'Võ Lê Ngọc', N'Tú', '1990-01-01', '2020-03-01', N'23 Cao Bá Quát, TP.Nha Trang', '0765133217');

select *from HangHoa
	INSERT INTO LoaiHang (MaLH, AnhLH, TenLH) VALUES
    ('LH001', N'mi_tom.jpg', N'Mì tôm'),
    ('LH002', N'gao.jpg', N'Gạo'),
    ('LH003', N'banh_mi.jpg', N'Bánh mì'),
    ('LH004', N'banh.jpg', N'Bánh'),
    ('LH005', N'keo.jpg', N'Kẹo'),
    ('LH006', N'kem.jpg', N'Kem'),
    ('LH007', N'sua_chua.jpg', N'Sữa chua'),
    ('LH008', N'sua.jpg', N'Sữa'),
    ('LH009', N'nuoc.jpg', N'Nước'),
    ('LH010', N'hat_nem.jpg', N'Hạt nêm'),
    ('LH011', N'mi_chinh.jpg', N'Mì chính'),
    ('LH012', N'dau_an.jpg', N'Dầu ăn'),
    ('LH013', N'tuong_ot.jpg', N'Tương ớt'),
    ('LH014', N'dau_goi.jpg', N'Dầu gội'),
    ('LH015', N'dau_xa.jpg', N'Dầu xả'),
    ('LH016', N'bot_giat.jpg', N'Bột giặt'),
    ('LH017', N'ban_chai.jpg', N'Bàn chải'),
    ('LH018', N'kem_danh_rang.jpg', N'Kem đánh răng'),
    ('LH019', N'xuc_xich.jpg', N'Xúc xích'),
    ('LH020', N'trung.jpg', N'Trứng'),

    ('LH021', N'rau.jpg', N'Rau'),
    ('LH022', N'trai_cay.jpg', N'Trái cây'),
    ('LH023', N'thuc_pham_tuoi_song.jpg', N'Thực phẩm tươi sống'),
    ('LH024', N'cafe.jpg', N'Cà phê'),
    ('LH025', N'tra.jpg', N'Trá'),
    ('LH026', N'nuoc_giai_khat.jpg', N'Nước giải khát'),
    ('LH027', N'bia.jpg', N'Bia'),

    ('LH029', N'but_viet.jpg', N'Bút viết'),
    ('LH030', N'khau_trang.jpg', N'Khẩu trang');


	INSERT INTO HangHoa (MaHH, MaLH, TenHH, AnhHH, SoLuongHangTon, GiaBan, HSD, NSX) VALUES
    ('HH001', 'LH001', N'Mì tôm Hảo Hảo', N'mi_tom_hao_hao.jpg', 100, 4000, '2024-01-21', '2023-06-22' ),
    ('HH002', 'LH001', N'Mì tôm Kokomi', N'mi_tom_kokomi.jpg', 200, 3000, '2024-02-26', '2023-07-27' ),
    ('HH003', 'LH002', N'Gạo thơm ST25', N'gao_thom_st25.jpg', 50, 159000, '2024-09-12', '2023-09-12' ),
    ('HH004', 'LH009', N'Nước ngọt Fanta hương soda kem trái cây 1.5 lít', N'fanta_soda_kem.jpg', 120, 20000, '2024-08-09', '2024-02-09'),
    ('HH005', 'LH009', N'Nước ngọt Sprite vị chanh', N'sprite_chanh.jpg', 150, 20000, '2024-09-14', '2024-03-14'),
    ('HH006', 'LH008', N'Thùng 48 hộp sữa tươi tiệt trùng Vinamilk 100% ít đường', N'vinamilk_sua_tieu_trung.jpg', 80, 385000, '2024-08-01', '2024-02-01'),
    ('HH007', 'LH008', N'Thùng 48 hộp sữa tươi TH True Milk có đường', N'th_true_milk.jpg', 70, 427000, '2024-09-05', '2024-03-05'),
    ('HH008', 'LH008', N'Thùng 24 bịch sữa tươi Dutch Lady không đường', N'dutch_lady_sua.jpg', 90, 169000, '2024-10-01', '2024-04-10'),
    ('HH009', 'LH003', N'Bánh mì tươi ổ sữa không nhân Kinh Đô 80g', N'banh_mi_kinh_do_sua.jpg', 300, 10000, '2024-03-21', '2024-03-10'),
    ('HH010', 'LH003', N'Bánh mì tươi ổ nhân Socola Kinh Đô 80g', N'banh_mi_kinh_do_socola.jpg', 200, 10000, '2024-03-19', '2024-03-08'),
    ('HH011', 'LH003', N'Bánh mì tươi chà bông Staff 65g', N'banh_mi_chabong_staff.jpg', 180, 8000, '2024-04-01', '2024-03-22'),
    ('HH012', 'LH002', N'Gạo lứt huyết rồng NutriChoice 0.5kg', N'gao_lut_nutrichoice.jpg', 60, 62000, '2024-04-12', '2023-10-15');



	INSERT INTO GioHang (MaKH, NgayTao) VALUES
    ('KH00001', '2024-12-01 10:00:00'),
    ('KH00002', '2024-12-02 15:30:00'),
    ('KH00003', '2024-12-03 18:45:00'),
    ('KH00004', '2024-12-04 08:20:00'),
    ('KH00005', '2024-12-05 13:50:00');

	select * from GioHang
	INSERT INTO ChiTietGioHang (MaGioHang, MaHH, SoLuong) VALUES
   
    (2, 'HH003', 2),
    (2, 'HH004', 1),
    (2, 'HH005', 6),
    (3, 'HH006', 1),
    (3, 'HH007', 2),
    (3, 'HH008', 3),
    (4, 'HH009', 10),
    (4, 'HH010', 8),
    (4, 'HH011', 4);
  

  	INSERT INTO Discounts (MaGiamGia, TenGiamGia, LoaiGiamGia, GiaTriGiamGia, NgayBatDau, NgayKetThuc)
VALUES
    ('GG001', N'Giảm 10% cho đơn hàng từ 500,000 VND', 'percent', 10.00, '2023-12-01', '2024-12-01'),
    ('GG002', N'Giảm 50,000 VND cho đơn hàng từ 200,000 VND', 'amount', 50000.00, '2023-12-01', '2024-12-01'),
    ('GG003', N'Giảm 20% cho sản phẩm sữa', 'percent', 20.00, '2023-12-01', '2024-12-01'),
    ('GG004', N'Giảm 30,000 VND cho đơn hàng từ 300,000 VND', 'amount', 30000.00, '2023-12-01', '2024-12-01'),
    ('GG005', N'Giảm 5% cho đơn hàng từ 1,000,000 VND', 'percent', 5.00, '2023-12-01', '2024-12-01'),
    ('GG006', N'Giảm 100,000 VND cho đơn hàng từ 500,000 VND', 'amount', 100000.00, '2023-12-01', '2024-12-01'),
    ('GG007', N'Giảm 15% cho sản phẩm thực phẩm', 'percent', 15.00, '2023-12-01', '2024-12-01'),
    ('GG008', N'Giảm 40,000 VND cho đơn hàng từ 250,000 VND', 'amount', 40000.00, '2023-12-01', '2024-12-01'),
    ('GG009', N'Giảm 25% cho các mặt hàng bánh kẹo', 'percent', 25.00, '2023-12-01', '2024-12-01'),
    ('GG010', N'Giảm 20,000 VND cho đơn hàng từ 150,000 VND', 'amount', 20000.00, '2023-12-01', '2024-12-01'),
    ('GG011', N'Giảm 10% cho đơn hàng từ 300,000 VND', 'percent', 10.00, '2023-12-01', '2024-12-01'),
    ('GG012', N'Giảm 50,000 VND cho sản phẩm điện tử', 'amount', 50000.00, '2023-12-01', '2024-12-01'),
    ('GG013', N'Giảm 30% cho các mặt hàng thời trang', 'percent', 30.00, '2023-12-01', '2024-12-01'),
    ('GG014', N'Giảm 40,000 VND cho đơn hàng từ 200,000 VND', 'amount', 40000.00, '2023-12-01', '2024-12-01'),
    ('GG015', N'Giảm 5% cho đơn hàng từ 1,500,000 VND', 'percent', 5.00, '2023-12-01', '2024-12-01'),
    ('GG016', N'Giảm 70,000 VND cho đơn hàng từ 400,000 VND', 'amount', 70000.00, '2023-12-01', '2024-12-01'),
    ('GG017', N'Giảm 15% cho sản phẩm mỹ phẩm', 'percent', 15.00, '2023-12-01', '2024-12-01'),
    ('GG018', N'Giảm 20,000 VND cho đơn hàng từ 100,000 VND', 'amount', 20000.00, '2023-12-01', '2024-12-01'),
    ('GG019', N'Giảm 25% cho các sản phẩm chăm sóc sức khỏe', 'percent', 25.00, '2023-12-01', '2024-12-01'),
    ('GG020', N'Giảm 10,000 VND cho đơn hàng từ 100,000 VND', 'amount', 10000.00, '2023-12-01', '2024-12-01'),
    ('GG021', N'Giảm 30% cho các mặt hàng thực phẩm chức năng', 'percent', 30.00, '2023-12-01', '2024-12-01'),
    ('GG022', N'Giảm 50,000 VND cho đơn hàng từ 500,000 VND', 'amount', 50000.00, '2023-12-01', '2024-12-01'),
    ('GG023', N'Giảm 20% cho sản phẩm đồ gia dụng', 'percent', 20.00, '2023-12-01', '2024-12-01'),
    ('GG024', N'Giảm 10,000 VND cho đơn hàng từ 50,000 VND', 'amount', 10000.00, '2023-12-01', '2024-12-01'),
    ('GG025', N'Giảm 15% cho sản phẩm đồ điện tử', 'percent', 15.00, '2023-12-01', '2024-12-01'),
    ('GG026', N'Giảm 70,000 VND cho đơn hàng từ 400,000 VND', 'amount', 70000.00, '2023-12-01', '2024-12-01'),
    ('GG027', N'Giảm 5% cho đơn hàng từ 1,000,000 VND', 'percent', 5.00, '2023-12-01', '2024-12-01'),
    ('GG028', N'Giảm 50,000 VND cho sản phẩm thực phẩm', 'amount', 50000.00, '2023-12-01', '2024-12-01'),
    ('GG029', N'Giảm 20% cho các sản phẩm điện máy', 'percent', 20.00, '2023-12-01', '2024-12-01'),
    ('GG030', N'Giảm 10% cho sản phẩm làm đẹp', 'percent', 10.00, '2023-12-01', '2024-12-01'),
    ('GG031', N'Giảm 30,000 VND cho đơn hàng từ 300,000 VND', 'amount', 30000.00, '2023-12-01', '2024-12-01'),
    ('GG032', N'Giảm 15% cho sản phẩm chăm sóc tóc', 'percent', 15.00, '2023-12-01', '2024-12-01');

	delete  from DonHang
	INSERT INTO DonHang (NgayDat, PhuongThucThanhToan, MaGiamGia, TongTien, TrangThai, MaKH) VALUES
    ('2024-12-05', N'Tiền mặt', 'GG001', 15000, N'Chờ xác nhận', 'KH00006'),
    ('2024-12-06', N'Chuyển khoản', NULL, 300000, N'Đã xác nhận', 'KH00002'),
    ('2024-12-07', N'Tiền mặt', 'GG002', 120000, N'Chờ xác nhận', 'KH00003'),
    ('2024-12-08', N'Tiền mặt', NULL, 450000, N'Đã hủy', 'KH00004'),
    ('2024-12-09', N'Tiền mặt', 'GG003', 220000, N'Đã xác nhận', 'KH00005');




	

















CREATE PROCEDURE HangHoa_TimKiem 
    @MaHH VARCHAR(10) = NULL, 
    @MaLH VARCHAR(10) = NULL, 
    @TenHH NVARCHAR(100) = NULL, 
    @GiaBanMin MONEY = NULL, 
    @GiaBanMax MONEY = NULL,
    @SoThang INT = NULL,
    @SoLuongMin INT = NULL,
    @SoLuongMax INT = NULL,
    @SapHetHang INT = NULL, -- Ngưỡng số lượng để cảnh báo sắp hết hàng
    @NgayNSXTu DATETIME = NULL,
    @NgayNSXDen DATETIME = NULL,
    @NgayHSDTu DATETIME = NULL,
    @NgayHSDDen DATETIME = NULL,
    @SapHetHan INT = NULL, -- Số ngày cảnh báo sắp hết hạn
    @CoHinhAnh BIT = NULL  -- 1: có hình, 0: không có hình, NULL: cả hai
AS 
BEGIN 
    DECLARE @SqlStr NVARCHAR(4000)
 
    SELECT @SqlStr = ' 
        SELECT *,
        DATEDIFF(MONTH, NSX, HSD) as ThoiHanSuDung,
        DATEDIFF(DAY, GETDATE(), HSD) as SoNgayConLai
        FROM HangHoa 
        WHERE (1=1) 
    ' 
 
    -- Các điều kiện tìm kiếm cũ
    IF @MaHH IS NOT NULL 
        SELECT @SqlStr = @SqlStr + ' 
            AND (MaHH LIKE N''%' + @MaHH + '%'') 
        ' 
 
    IF @MaLH IS NOT NULL 
        SELECT @SqlStr = @SqlStr + ' 
            AND (MaLH LIKE N''%' + @MaLH + '%'') 
        ' 
 
    IF @TenHH IS NOT NULL 
        SELECT @SqlStr = @SqlStr + ' 
            AND (TenHH LIKE N''%' + @TenHH + '%'') 
        ' 
 
    IF @GiaBanMin IS NOT NULL AND @GiaBanMax IS NOT NULL 
        SELECT @SqlStr = @SqlStr + ' 
            AND (GiaBan BETWEEN ' + CAST(@GiaBanMin AS NVARCHAR) + ' AND ' + CAST(@GiaBanMax AS NVARCHAR) + ') 
        ' 

    IF @SoThang IS NOT NULL 
        SELECT @SqlStr = @SqlStr + ' 
            AND (DATEDIFF(MONTH, NSX, HSD) = ' + CAST(@SoThang AS NVARCHAR) + ') 
        ' 

    -- Thêm các điều kiện tìm kiếm mới
    -- Tìm theo số lượng tồn kho
    IF @SoLuongMin IS NOT NULL AND @SoLuongMax IS NOT NULL
        SELECT @SqlStr = @SqlStr + '
            AND (SoLuongHangTon BETWEEN ' + CAST(@SoLuongMin AS NVARCHAR) + ' AND ' + CAST(@SoLuongMax AS NVARCHAR) + ')
        '

    -- Tìm hàng sắp hết
    IF @SapHetHang IS NOT NULL
        SELECT @SqlStr = @SqlStr + '
            AND (SoLuongHangTon <= ' + CAST(@SapHetHang AS NVARCHAR) + ')
        '

    -- Tìm theo khoảng thời gian NSX
    IF @NgayNSXTu IS NOT NULL AND @NgayNSXDen IS NOT NULL
        SELECT @SqlStr = @SqlStr + '
            AND (NSX BETWEEN ''' + CAST(@NgayNSXTu AS NVARCHAR) + ''' AND ''' + CAST(@NgayNSXDen AS NVARCHAR) + ''')
        '

    -- Tìm theo khoảng thời gian HSD
    IF @NgayHSDTu IS NOT NULL AND @NgayHSDDen IS NOT NULL
        SELECT @SqlStr = @SqlStr + '
            AND (HSD BETWEEN ''' + CAST(@NgayHSDTu AS NVARCHAR) + ''' AND ''' + CAST(@NgayHSDDen AS NVARCHAR) + ''')
        '

    -- Tìm hàng sắp hết hạn
    IF @SapHetHan IS NOT NULL
        SELECT @SqlStr = @SqlStr + '
            AND (DATEDIFF(DAY, GETDATE(), HSD) <= ' + CAST(@SapHetHan AS NVARCHAR) + ')
        '

    -- Tìm theo tình trạng hình ảnh
    IF @CoHinhAnh IS NOT NULL
        SELECT @SqlStr = @SqlStr + '
            AND (AnhHH IS ' + CASE WHEN @CoHinhAnh = 1 THEN 'NOT NULL' ELSE 'NULL' END + ')
        '
 
    -- Thực thi câu lệnh SQL động 
    EXEC SP_EXECUTESQL @SqlStr 
END




-- Trigger kiểm tra số lượng tồn khi thêm vào giỏ hàng




DROP VIEW v_DoanhThuNgay;

-- 1. Tạo view thống kê doanh thu theo ngày
CREATE VIEW v_DoanhThuNgay AS
SELECT 
    CAST(NgayGD AS DATE) AS Ngay,
    COUNT(DISTINCT hd.SoHD) AS SoDonHang,
    SUM(cthd.SoLuong * cthd.GiaBan) AS DoanhThu,
    SUM(cthd.SoLuong) AS TongSoLuong
FROM HoaDon hd
JOIN ChiTietHoaDon cthd ON hd.SoHD = cthd.SoHD
GROUP BY CAST(NgayGD AS DATE);

-- 2. Tạo view thống kê doanh thu theo tháng

DROP VIEW v_DoanhThuThang
CREATE VIEW v_DoanhThuThang AS
SELECT 
    YEAR(NgayGD) AS Nam,
    MONTH(NgayGD) AS Thang,
    COUNT(DISTINCT hd.SoHD) AS SoDonHang,
    SUM(cthd.SoLuong * cthd.GiaBan) AS DoanhThu,
    SUM(cthd.SoLuong) AS TongSoLuong
FROM HoaDon hd
JOIN ChiTietHoaDon cthd ON hd.SoHD = cthd.SoHD
GROUP BY YEAR(NgayGD), MONTH(NgayGD);


DROP PROCEDURE sp_ThongKeDoanhThu;

-- 3. Stored Procedure thống kê doanh thu theo khoảng thời gian
CREATE PROCEDURE sp_ThongKeDoanhThu
    @TuNgay DATE,
    @DenNgay DATE,
    @LoaiThongKe VARCHAR(20) = 'ngay' -- 'ngay' hoặc 'thang'
AS
BEGIN
    IF @LoaiThongKe = 'ngay'
    BEGIN
        SELECT 
            CAST(NgayGD AS DATE) AS Ngay,
            COUNT(DISTINCT hd.SoHD) AS SoDonHang,
            SUM(cthd.SoLuong * cthd.GiaBan) AS DoanhThu,
            SUM(cthd.SoLuong) AS TongSoLuong
        FROM HoaDon hd
        JOIN ChiTietHoaDon cthd ON hd.SoHD = cthd.SoHD
        WHERE CAST(NgayGD AS DATE) BETWEEN @TuNgay AND @DenNgay
        GROUP BY CAST(NgayGD AS DATE)
        ORDER BY Ngay;
    END
    ELSE
    BEGIN
        SELECT 
            YEAR(NgayGD) AS Nam,
            MONTH(NgayGD) AS Thang,
            COUNT(DISTINCT hd.SoHD) AS SoDonHang,
            SUM(cthd.SoLuong * cthd.GiaBan) AS DoanhThu,
            SUM(cthd.SoLuong) AS TongSoLuong
        FROM HoaDon hd
        JOIN ChiTietHoaDon cthd ON hd.SoHD = cthd.SoHD
        WHERE CAST(NgayGD AS DATE) BETWEEN @TuNgay AND @DenNgay
        GROUP BY YEAR(NgayGD), MONTH(NgayGD)
        ORDER BY Nam, Thang;
    END
END;



drop PROCEDURE sp_ThongKeDoanhThuTheoSanPham
-- 4. Stored Procedure thống kê doanh thu theo sản phẩm
CREATE PROCEDURE sp_ThongKeDoanhThuTheoSanPham
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SELECT 
        hh.MaHH,
        hh.TenHH,
        lh.TenLH AS LoaiHang,
        SUM(cthd.SoLuong) AS TongSoLuong,
        SUM(cthd.SoLuong * cthd.GiaBan) AS DoanhThu
    FROM HoaDon hd
    JOIN ChiTietHoaDon cthd ON hd.SoHD = cthd.SoHD
    JOIN HangHoa hh ON cthd.MaHH = hh.MaHH
    JOIN LoaiHang lh ON hh.MaLH = lh.MaLH
    WHERE CAST(NgayGD AS DATE) BETWEEN @TuNgay AND @DenNgay
    GROUP BY hh.MaHH, hh.TenHH, lh.TenLH
    ORDER BY DoanhThu DESC;
END;


drop PROCEDURE sp_ThongKeDoanhThuTheoLoaiHang
-- 5. Stored Procedure thống kê doanh thu theo loại hàng
CREATE PROCEDURE sp_ThongKeDoanhThuTheoLoaiHang
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SELECT 
        lh.MaLH,
        lh.TenLH,
        COUNT(DISTINCT hh.MaHH) AS SoSanPham,
        SUM(cthd.SoLuong) AS TongSoLuong,
        SUM(cthd.SoLuong * cthd.GiaBan) AS DoanhThu
    FROM HoaDon hd
    JOIN ChiTietHoaDon cthd ON hd.SoHD = cthd.SoHD
    JOIN HangHoa hh ON cthd.MaHH = hh.MaHH
    JOIN LoaiHang lh ON hh.MaLH = lh.MaLH
    WHERE CAST(NgayGD AS DATE) BETWEEN @TuNgay AND @DenNgay
    GROUP BY lh.MaLH, lh.TenLH
    ORDER BY DoanhThu DESC;
END;


drop PROCEDURE sp_TopSanPhamBanChay
-- 6. Stored Procedure thống kê top sản phẩm bán chạy
CREATE PROCEDURE sp_TopSanPhamBanChay
    @TuNgay DATE,
    @DenNgay DATE,
    @TopN INT = 10
AS
BEGIN
    SELECT TOP (@TopN)
        hh.MaHH,
        hh.TenHH,
        lh.TenLH AS LoaiHang,
        SUM(cthd.SoLuong) AS TongSoLuong,
        SUM(cthd.SoLuong * cthd.GiaBan) AS DoanhThu
    FROM HoaDon hd
    JOIN ChiTietHoaDon cthd ON hd.SoHD = cthd.SoHD
    JOIN HangHoa hh ON cthd.MaHH = hh.MaHH
    JOIN LoaiHang lh ON hh.MaLH = lh.MaLH
    WHERE CAST(NgayGD AS DATE) BETWEEN @TuNgay AND @DenNgay
    GROUP BY hh.MaHH, hh.TenHH, lh.TenLH
    ORDER BY TongSoLuong DESC;
END;




-- Add TrangThaiGiaoHang column to DonHang table


-- Modify XuLyDonHang stored procedure to handle delivery status

SELECT * FROM DonHang WHERE MaKH NOT IN (SELECT MaKH FROM KhachHang);
SELECT * FROM ChiTietDonHang WHERE MaDH NOT IN (SELECT MaDH FROM DonHang)
   OR MaHH NOT IN (SELECT MaHH FROM HangHoa);

   SELECT * FROM HoaDon WHERE MaKH NOT IN (SELECT MaKH FROM KhachHang)
   OR MaNV NOT IN (SELECT MaNV FROM NhanVien)
   OR MaDH NOT IN (SELECT MaDH FROM DonHang);





   -- Chạy câu SQL này để xem data
SELECT gh.*, ctgh.*, hh.*
FROM GioHang gh
LEFT JOIN ChiTietGioHang ctgh ON gh.MaGioHang = ctgh.MaGioHang 
LEFT JOIN HangHoa hh ON ctgh.MaHH = hh.MaHH
WHERE gh.MaKH = 'giá_trị_session_MaKH'


-- Kiểm tra từng bảng
SELECT * FROM GioHang;  -- Xem tất cả giỏ hàng
SELECT * FROM ChiTietGioHang;  -- Xem chi tiết
SELECT * FROM HangHoa;  -- Xem hàng hóa

-- Kiểm tra relationships
SELECT gh.MaGioHang, gh.MaKH, ctgh.MaHH 
FROM GioHang gh
LEFT JOIN ChiTietGioHang ctgh ON gh.MaGioHang = ctgh.MaGioHang;


SELECT * FROM GioHang WHERE MaKH = 'KH00001' -- Thay bằng giá trị session thực tế



SELECT COLUMN_NAME, IS_IDENTITY
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'DonHangs' AND COLUMN_NAME = 'MaDH'
