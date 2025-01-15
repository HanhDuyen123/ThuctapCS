CREATE DATABASE ThucTapCS;
GO

USE ThucTapCS;

-- Bảng Customer
CREATE TABLE Customer (
    customer_id BIGINT IDENTITY(1,1) PRIMARY KEY,  -- Tự động tăng sử dụng IDENTITY
    first_name NVARCHAR(30) NOT NULL,
    last_name NVARCHAR(50) NOT NULL,
    date_of_birth DATETIME,
    gender NVARCHAR(3),
    email NVARCHAR(50) UNIQUE NOT NULL,
    password NVARCHAR(255) NOT NULL,
    phone NVARCHAR(15) UNIQUE,
    address NVARCHAR(255),
    registration_date DATETIME DEFAULT GETDATE()  -- Sử dụng GETDATE() để lấy thời gian hiện tại
);

-- Bảng Employee
CREATE TABLE Employee (
    employee_id BIGINT IDENTITY(1,1) PRIMARY KEY,  -- Tự động tăng sử dụng IDENTITY
    first_name NVARCHAR(30) NOT NULL,
    last_name NVARCHAR(50) NOT NULL,
    date_of_birth DATETIME,
    gender NVARCHAR(3),
    email NVARCHAR(50) UNIQUE NOT NULL,
    password NVARCHAR(255) NOT NULL,
    phone NVARCHAR(15) UNIQUE,
    address NVARCHAR(255),
    role_name NVARCHAR(100) NOT NULL,
    hire_date DATETIME DEFAULT GETDATE(),  -- Sử dụng GETDATE() để lấy thời gian hiện tại
    salary DECIMAL(10, 2) NOT NULL  -- Thêm thông tin lương
);

CREATE TABLE Ward (
    ward_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    ward_name NVARCHAR(255) NOT NULL,  -- Tên phường xã
    district_name NVARCHAR(255) NOT NULL DEFAULT 'Cam Lâm',  -- Mặc định là Cam Lâm
    province_name NVARCHAR(255) NOT NULL DEFAULT 'Khánh Hòa'  -- Mặc định là Khánh Hòa
);

-- Bảng Order
CREATE TABLE [Order] (
    order_id BIGINT IDENTITY(1,1) PRIMARY KEY,  -- Tự động tăng sử dụng IDENTITY
    customer_id BIGINT NOT NULL,  -- Chỉ lưu thông tin người gửi
    receiver_name NVARCHAR(100) NOT NULL,  -- Tên người nhận
    receiver_street NVARCHAR(255) NOT NULL,  -- Địa chỉ chi tiết người nhận (số nhà, tên đường)
    ward_id BIGINT,  -- Liên kết đến phường xã
    receiver_phone NVARCHAR(15) NOT NULL,  -- Số điện thoại người nhận
    order_price DECIMAL(11, 2) NOT NULL,  -- Tổng giá trị đơn hàng
    shipping_fee DECIMAL(11, 2) NOT NULL,  -- Phí vận chuyển
    warehouse_date DATETIME,
    shipping_date DATETIME,
    status NVARCHAR(20) CHECK (status IN ('Processing', 'Shipping', 'Delivered', 'Returned')) DEFAULT 'Processing',
    return_reason NVARCHAR(255),
    FOREIGN KEY (customer_id) REFERENCES Customer(customer_id) ON DELETE CASCADE,
    FOREIGN KEY (ward_id) REFERENCES Ward(ward_id) ON DELETE SET NULL  
);

-- Bảng phân công công việc theo từng đơn hàng
CREATE TABLE OrderAssignment (
    assignment_id BIGINT IDENTITY(1,1) PRIMARY KEY,  -- Tự động tăng ID
    employee_id BIGINT NOT NULL,  -- Nhân viên được phân công công việc
    order_id BIGINT NOT NULL,  -- Liên kết với bảng Order để chỉ ra đơn hàng nào được giao
    assigned_date DATETIME DEFAULT GETDATE(),  -- Ngày giao mặc định là ngày hiện tại
    FOREIGN KEY (employee_id) REFERENCES Employee(employee_id) ON DELETE CASCADE,  -- Liên kết với bảng Employee
    FOREIGN KEY (order_id) REFERENCES [Order](order_id) ON DELETE NO ACTION  -- Liên kết với bảng Order
);


INSERT INTO Customer (first_name, last_name, date_of_birth, gender, email, password, phone, address)
VALUES 
(N'Hiền', N'Dương Văn', '1990-01-01', N'Nam', 'dvhien@gmail.com', 'a21d0b715ba1f3d491a98c24b2437c41', '0914386529', N'12 Trần Phú, Nha Trang'),
(N'Thắm', N'Nguyễn Hồng', '1995-02-15', N'Nữ', 'nhtham@gmail.com', '51b6c5ee1817e265c7edf7b33a0ba2f9', '0816352483', N'31 Nguyễn Trãi, Nha Trang'),
(N'Yến', N'Trần Hải', '2000-03-20', N'Nữ', 'thyen@gmail.com', '2db6d7fe11bcf5de872abd8985ec3dad', '0707138261', N'98 Lý Thánh Tôn, Hà Nội'),
(N'Cường', N'Trần Chí', '1990-01-01', N'Nam', 'tccuong@gmail.com', '5292c575331863481e158b58acb60abc', '0919386729', N'120 Trần Phú, Đà Nẵng'),
(N'Uyên', N'Phạm Hồng', '1995-09-17', N'Nữ', 'phuyen@gmail.com', '3afc74b8661645008684f1e05b528196', '0810352383', N'91 Nguyễn Trãi, Hội An'),
(N'Ly', N'Lương Yên', '2003-07-20', N'Nữ', 'lyly@gmail.com', '8a1227fa35289eec0d11321758a4c169', '0707738161', N'223 Nguyễn Khuyến, Cần Thơ'),
(N'Nam', N'Nguyễn Hoài', '1993-12-01', N'Nam', 'nhnam@gmail.com', 'f6a87b929c297298b9dbff294ee32e20', '0814386229', N'58 Trần Phú, Hà Nội'),
(N'Quyền', N'Lê Ngọc', '1998-02-20', N'Nữ', 'lnquyen@gmail.com', '677055cbb393146d4780b6c93271d457', '0813356403', N'109 Phạm Văn Đồng, TP.HCM'),
(N'Châu', N'Nguyễn Trần Minh', '2001-09-20', N'Nữ', 'ntmchau@gmail.com', 'cbb09835d781f8c4d8b0da7e91df50ee', '0708138261', N'98 Lê Thánh Tôn, Hà Nội'),
(N'Anh', N'Dương Trọng', '1997-01-09', N'Nam', 'dtanh@gmail.com', '5071379a85924394b45bfff4bfb51583', '0915385529', N'102 Trần Phú, Nha Trang');

INSERT INTO Employee (first_name, last_name, date_of_birth, gender, email, password, phone, address, role_name, hire_date, salary)
VALUES 
(N'Duy', N'Phạm Minh', '1992-04-06', N'Nam', 'pmduy@gmail.com', '86cc5c3879615619e79817e89fa7b97f', '0912624188', N'17 Trần Quý Cáp, Cam Hòa', N'Quản lý', '2019-05-06', 12000000),
(N'Hòa', N'Trần Đức', '1980-07-15', 'Nam', 'duchoa@gmail.com', 'eed0ba03e0ef2658b921437f22e0650f', '0972971699', N'81 Nguyễn Thị Minh Khai, Cam Đức', N'Quản lý', '2019-04-10',12000000),
(N'Hưng', N'Nguyễn Trần Vĩnh', '1990-05-05', N'Nam', 'ntvhung@gmail.com', 'b53b2c315fb81984f5d54da7940cbc3f', '0923956489', N'78 Đặng Tất, Cam Đức', N'Nhân viên', '2023-09-18', 8000000),
(N'Huy', N'Hà Quang', '1999-05-07', N'Nam', 'hqhuy@gmail.com', '5c570646a7362c5073726d67a8d622ae', '0893827164', N'12 Nguyễn Thị Minh Khai, Cam Đức', N'Nhân viên', '2020-09-10', 8000000),
(N'Quân', N'Cao Dương', '1994-08-05', N'Nam', 'cdquan@gmail.com', '526a686e48b174e7a88673d6e18caca3', '0899672813', N'76 Lý Phục Man, Cam Hải Tây', N'Nhân viên', '2021-05-20', 8000000),
(N'Hải', N'Ngô', '1997-07-12', N'Nam', 'nhai@gmail.com', '7649878fe515ddaf951caf48d2ce3a9e', '0798273102', N'90 Nguyễn Chánh, Cam Tân', N'Nhân viên', '2021-05-20', 8000000),
(N'Tín', N'Nguyễn Hữu', '1997-05-11', N'Nam', 'nhtin@gmail.com', '076df6fab1e5ded70d788cde52a72ec6', '0927182492', N'28 Trường Chinh, Cam Hiệp', N'Nhân viên', '2021-05-20', 8000000),
(N'Kiệt', N'Phạm Minh', '2000-05-06', N'Nam', 'pmkiet@gmail.com', 'f240587fe88867bf96022e91ad37fc5c', '0912634108', N'171 Trần Quý Cáp, Cam Thành Bắc', N'Nhân viên', '2020-09-10', 8000000),
(N'Lâm', N'Lê Đức', '1990-07-15', N'Nam', 'ldlam@gmail.com', '32f4e3da5c7756343c8bd63178b723b7', '0982976699', N'24 Nguyễn Thị Minh Khai, Cam Đức', N'Nhân viên', '2023-09-10', 8000000),
(N'Anh', N'Nguyễn Trần Hải', '1990-05-05', N'Nam', 'ngtrhanh@gmail.com', '0c25ad3052a5715926ba6bc4e6b6d4dc', '0823456729', N'18 Đặng Tất, Cam Đức', N'Nhân viên', '2021-05-20', 8000000),
(N'Linh', N'Lê Quang', '1994-05-07', N'Nam', 'lquanglinh@gmail.com', 'dead77dbbd624786eb09b00542414813', '0892826164', N'89C Nguyễn Thị Minh Khai, Cam Đức', N'Nhân viên', '2020-09-10', 8000000),
(N'Quan', N'Dương Hà', '1999-10-05', N'Nam', 'dhaquan@gmail.com', 'd35ccb408094678caafd5cba512b5010', '0898672823', N'Bãi Giếng, Cam Hải Tây', N'Nhân viên', '2020-09-10', 8000000),
(N'Minh', N'Nguyễn Trí', '1997-06-12', N'Nam', 'ntminh@gmail.com', 'c262ee67594e08b00632d2156ca590d0', '0298273112', N'16 Nguyễn Chánh, Cam Tân', N'Nhân viên', '2020-09-10', 8000000),
(N'Tính', N'Hà Thanh', '1997-06-21', N'Nam', 'httin@gmail.com', '7cdc3db813a34dd8a46b7e2f7a252e6d', '0727181492', N'20 Nguyễn Đình Chiểu, Cam Tân', N'Nhân viên', '2020-09-10', 8000000),
(N'Toàn', N'Đỗ Đức', '1999-12-06', N'Nam', 'dductoan@gmail.com', 'ac63360dde19faadcc2d1fab71f55859', '0912424189', N'162 Trần Quý Cáp, Cam Hòa', N'Nhân viên', '2023-09-10', 8000000),
(N'Ân', N'Nguyễn Hoài', '1990-07-25', 'Nam', 'nhoaian@gmail.com', '952a5d62a5d1534fe6a35e87c4fe0993', '0973971629', N'01 Nguyễn Thị Minh Khai, Cam Đức', N'Nhân viên', '2020-09-10', 8000000),
(N'An', N'Nguyễn Vĩnh', '1993-09-05', N'Nam', 'nvan@gmail.com', 'ee1f5d96c0efeca221ddfee8cd8cbae0', '0921456680', N'Hương Lộ Ngọc Hiệp, Cam Đức', N'Nhân viên', '2021-05-20', 8000000),
(N'Tú', N'Lê Tuấn', '1996-05-27', N'Nam', 'ltuan@gmail.com', '704767ce12a38c613d1e02a26ffd8ddc', '0897827161', N'12 Võ Thị Sáu, Cam Đức', N'Nhân viên', '2023-09-10', 8000000),
(N'Khánh', N'Tạ Gia', '1995-12-05', N'Nam', 'takhanh@gmail.com', '7c045efec2076321d9018d080990551f', '0998672813', N'Bãi Giếng 1, Cam Hải Tây', N'Nhân viên', '2023-09-10', 8000000);

INSERT INTO Ward (ward_name, district_name, province_name)
VALUES 
(N'Cam Đức', 'Cam Lâm', 'Khánh Hòa'),
(N'Cam An Bắc', 'Cam Lâm', 'Khánh Hòa'),
(N'Cam An Nam', 'Cam Lâm', 'Khánh Hòa'),
(N'Cam Hải Đông', 'Cam Lâm', 'Khánh Hòa'),
(N'Cam Hải Tây', 'Cam Lâm', 'Khánh Hòa'),
(N'Cam Hiệp Bắc', 'Cam Lâm', 'Khánh Hòa'),
(N'Cam Hiệp Nam', 'Cam Lâm', 'Khánh Hòa'),
(N'Cam Hòa', 'Cam Lâm', 'Khánh Hòa'),
(N'Cam Phước Tây', 'Cam Lâm', 'Khánh Hòa'),
(N'Cam Tân', 'Cam Lâm', 'Khánh Hòa'),
(N'Cam Thành Bắc', 'Cam Lâm', 'Khánh Hòa'),
(N'Sơn Tân', 'Cam Lâm', 'Khánh Hòa'),
(N'Suối Cát', 'Cam Lâm', 'Khánh Hòa'),
(N'Suối Tân', 'Cam Lâm', 'Khánh Hòa');

INSERT INTO [Order] (customer_id, receiver_name, receiver_street, ward_id, receiver_phone, order_price, shipping_fee, status)
VALUES 
(1, N'Trần Trung Trí', N'123 Phan Đình Phùng', 11, '0901618274', 523000, 20000, N'Processing'),
(2, N'Nguyễn Văn Chí', N'42 Nguyễn Huệ', 2, '0901927511', 750000, 3000, N'Processing'),
(3, N'Lê Thị Diễm', N'79 Lê Lai', 3, '0917104725', 617000, 0, N'Processing'),
(4, N'Phạm Văn Hinh', N'101 Trần Hưng Đạo', 4, '0904567890', 4000000, 0, N'Processing'),
(5, N'Trần Thị Hạnh', N'22 Hoàng Diệu', 1, '0905678901', 17000, 35000, N'Processing'),
(6, N'Nguyễn Hoài Dương', N'198 Phan Văn Trị', 6, '0284183023', 90000, 5000, N'Processing'),
(7, N'Lê Chí Trung', N'10 Đinh Tiên Hoàng', 1, '0987103628', 650000, 0, N'Processing'),
(8, N'Nguyễn Thị Hương', N'52/10 Hàm Nghi', 8, '0980163418', 700000, 3000, N'Processing'),
(9, N'Nguyễn Văn Bình', N'6C/102 Trương Định', 9, '0879176391', 450000, 15000, N'Processing'),
(10, N'Trần Ngọc Thanh', N'70 Võ Văn Kiệt', 10, '0982017630', 1500000, 15000, N'Processing'),
(1, N'Lê Trần Bảo Anh', N'08 Nguyễn Thái Học', 5, '0782301547', 5000, 20000, N'Processing'),
(2, N'Nguyễn Thị Hồng', N'9 Lý Tự Trọng', 12, '0981274527', 37162, 10000, N'Processing'),
(3, N'Trần Văn Minh', N'11 Lý Tự Trọng', 12, '0901832841', 90400, 35000, N'Processing'),
(4, N'Lê Thị Ngọc Hà', N'20 Trần Quốc Toản', 7, '0904510423', 85772, 30000, N'Processing'),
(5, N'Nguyễn Hoài Dương', N'198 Phan Văn Trị', 6, '0284183023', 410725, 15000, N'Processing'),
(6, N'Trần Thị Hạnh', N'22 Hoàng Diệu', 1, '0905678901', 77000, 25000, N'Processing'),
(7, N'Trần Trung Trực', N'53 Bùi Viện', 5, '0987120462', 800100, 0, N'Processing'),
(8, N'Nguyễn Thanh Thái', N'6/12 Đinh Bộ Lĩnh', 6, '0908183402', 95000, 10000, N'Processing'),
(9, N'Trần Thu', N'79 Phan Văn Trị', 6, '0782638013', 601000, 2000, N'Processing'),
(10, N'Lê Văn Bửi', N'Gần trường Trần Bình Trọng', 1, '0912743909', 550000, 15000, N'Processing'),
(1, N'Nguyễn Hàng Gia', N'99 Nguyễn Văn Cừ', 9, '0812836186', 75000, 17100, N'Processing'),
(2, N'Trần Trí Cường', N'110 Nguyễn Thị Minh Khai', 1, '0892536816', 450000, 15000, N'Processing'),	
(3, N'Lê Thu Linh', N'11 Phan Bội Châu', 12, '0903977284', 600000, 20000, N'Processing'),
(4, N'Lê Thu Linh', N'11 Phan Bội Châu', 12, '0903977284', 700000, 30000, N'Processing'),
(5, N'Trần Văn Ý', N'133 Đinh Tiên Hoàng', 1, '0916352947', 800000, 35000, N'Processing'),
(6, N'Lê Khanh', N'141 Trường Chinh', 1, '0906159016', 550000, 15000, N'Processing'),
(7, N'Nguyễn Trần Hương Ngọc', N'151 Đinh Tiên Hoàng', 1, '0981920473', 750000, 25000, N'Processing'),
(8, N'Trần Chính Thương', N'16 Lê Thị Hồng Gấm', 1, '0908826194', 500000, 20000, N'Processing'),
(9, N'Trần Chính Thương', N'16 Lê Thị Hồng Gấm', 1, '0908826194', 600000, 30000, N'Processing'),
(10, N'Trần Chính Dương', N'16 Lê Thị Hồng Gấm', 1, '0908826194', 700000, 40000, N'Processing'),
(1, N'Trần Hoàng Hảo', N'99 Lê Hồng Phong', 14, '0987126430', 850000, 25000, N'Processing');

INSERT INTO OrderAssignment (employee_id, order_id, assigned_date)
VALUES 
(3, 1, GETDATE()),
(4, 2, GETDATE()),
(5, 3, GETDATE()),
(6, 4, GETDATE()),
(7, 5, GETDATE()),
(8, 6, GETDATE()),
(7, 7, GETDATE()),
(9, 8, GETDATE()),
(10, 9, GETDATE()),
(11, 10, GETDATE()),
(12, 11, GETDATE()),
(13, 12, GETDATE()),
(13, 13, GETDATE()),
(14, 14, GETDATE()),
(8, 15, GETDATE()),
(7, 16, GETDATE()),
(12, 17, GETDATE()),
(8, 18, GETDATE()),
(8, 19, GETDATE()),
(7, 20, GETDATE()),
(10, 21, GETDATE()),
(7, 22, GETDATE()),
(13, 23, GETDATE()),
(13, 24, GETDATE()),
(7, 25, GETDATE()),
(7, 26, GETDATE()),
(7, 27, GETDATE()),
(7, 28, GETDATE()),
(7, 29, GETDATE()),
(7, 30, GETDATE()),
(15, 31, GETDATE());
SELECT * FROM [Order];
