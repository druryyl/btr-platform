-- ============================================================
-- BTR Database Creation Script
-- Database Name: btr2
-- Server: JUDE7 (localhost)
-- Generated: 2026-09-24
-- Source: src/j05-btr-distrib/btr.sql/Tables
-- ============================================================

USE master;
GO

-- Drop database if exists (uncomment if needed)
/* 
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'btr2')
BEGIN
    ALTER DATABASE [btr2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [btr2];
END
GO
*/

-- Create database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'btr2')
BEGIN
    CREATE DATABASE [btr2];
END
GO

USE [btr2];
GO

-- ============================================================
-- TABLE CREATION SCRIPTS
-- ============================================================


-- ============================================================
-- File: \BrgContext\BTR_Brg.sql
-- ============================================================

CREATE TABLE BTR_Brg(
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_Brg_BrgId DEFAULT(''),
    BrgName VARCHAR(60) NOT NULL CONSTRAINT DF_BTR_Brg_BrgName DEFAULT(''),
    BrgCode VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Brg_BrgCode DEFAULT(''),
    IsAktif BIT NOT NULL CONSTRAINT DF_BTR_Brg_IsAktif DEFAULT(0),

    SupplierId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Brg_SupplierId DEFAULT(''),
    JenisBrgId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_Brg_JenisBrgId DEFAULT(''), 
    KategoriId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Brg_KategoriId DEFAULT(''),

    Hpp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Brg_Hpp DEFAULT(0),
    HppTimestamp DATETIME NOT NULL CONSTRAINT DF_BTR_Brg_HppTimestamp DEFAULT('3000-01-01'),
    IdBarang INT NOT NULL CONSTRAINT DF_BTR_Brg_IdBarang DEFAULT(0),

    CONSTRAINT PK_BTR_Brg PRIMARY KEY CLUSTERED (BrgId)
)
GO

CREATE INDEX IX_BTR_Brg_BrgCode
    ON BTR_Brg(BrgCode, BrgId)
    WITH(FILLFACTOR=75)
GO

-- ============================================================
-- File: \BrgContext\BTR_BrgBarcode.sql
-- ============================================================

CREATE TABLE BTR_BrgBarcode(
    BrgBarcodeId   VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_BrgBarcodeId   DEFAULT(''),
    BarcodeValue   VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_BarcodeValue   DEFAULT(''),
    BarcodeValueKey AS UPPER(LTRIM(RTRIM(BarcodeValue))) PERSISTED,
    BrgId          VARCHAR(6)  NOT NULL CONSTRAINT DF_BTR_BrgBarcode_BrgId          DEFAULT(''),
    Satuan         VARCHAR(7)  NOT NULL CONSTRAINT DF_BTR_BrgBarcode_Satuan         DEFAULT(''),
    IsAktif        BIT         NOT NULL CONSTRAINT DF_BTR_BrgBarcode_IsAktif        DEFAULT(0),

    CreatedBy      VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_CreatedBy      DEFAULT(''),
    CreatedDate    DATETIME    NOT NULL CONSTRAINT DF_BTR_BrgBarcode_CreatedDate    DEFAULT('3000-01-01'),
    ModifiedBy     VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_ModifiedBy     DEFAULT(''),
    ModifiedDate   DATETIME    NOT NULL CONSTRAINT DF_BTR_BrgBarcode_ModifiedDate   DEFAULT('3000-01-01'),

    RowVer         ROWVERSION     NOT NULL,

    CONSTRAINT PK_BTR_BrgBarcode PRIMARY KEY CLUSTERED (BrgBarcodeId)
)
GO

CREATE UNIQUE INDEX UX_BTR_BrgBarcode_BarcodeValueKey
    ON BTR_BrgBarcode(BarcodeValueKey)
    WITH(FILLFACTOR=75)
GO

CREATE INDEX IX_BTR_BrgBarcode_BrgId
    ON BTR_BrgBarcode(BrgId, BrgBarcodeId)
    WITH(FILLFACTOR=75)
GO

CREATE INDEX IX_BTR_BrgBarcode_RowVer
    ON BTR_BrgBarcode(RowVer)
GO

ALTER TABLE BTR_BrgBarcode
    ADD CONSTRAINT FK_BTR_BrgBarcode_BTR_Brg
    FOREIGN KEY (BrgId) REFERENCES BTR_Brg(BrgId)
GO


-- ============================================================
-- File: \BrgContext\BTR_BrgHarga.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_BrgHarga]
(
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_BrgHarga_BrgId DEFAULT(''), 
    HargaTypeId VARCHAR(2) NOT NULL CONSTRAINT DF_BTR_BrgHarga_HargaTypeId DEFAULT(''), 
    Harga DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_BrgHarga_Harga DEFAULT(0), 
    HargaTimestamp DATETIME NOT NULL CONSTRAINT DF_BTR_BrgHarga_HargaTimestamp DEFAULT('3000-01-01'),
    
    CONSTRAINT PK_BTR_BrgHarga PRIMARY KEY CLUSTERED(BrgId, HargaTypeId)
)


-- ============================================================
-- File: \BrgContext\BTR_BrgSatuan.sql
-- ============================================================

CREATE TABLE BTR_BrgSatuan(
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_BrgSatuanHarga_BrgId DEFAULT(''), 
    Satuan VARCHAR(7) NOT NULL CONSTRAINT DF_BTR_BrgSatuanHarga_Satuan DEFAULT(''),
    Conversion INT NOT NULL CONSTRAINT DF_BTR_BrgSatuanHarga_Conversion DEFAULT(0),
    SatuanPrint VARCHAR(7) NOT NULL CONSTRAINT DF_BTR_BrgSatuan_SatuanPrint DEFAULT(''),

    CONSTRAINT PK_BrgSatuan PRIMARY KEY CLUSTERED (BrgId, Satuan)
)

-- ============================================================
-- File: \BrgContext\BTR_HargaType.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_HargaType]
(
	HargaTypeId VARCHAR(2) NOT NULL CONSTRAINT DF_BTR_HargaType_HargaTypeId DEFAULT(''),
	HargaTypeName VARCHAR(15) NOT NULL CONSTRAINT DF_BTR_HargaType_HargaTypeName DEFAULT(''),
	Margin DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_HargaType_Margin DEFAULT(0),
	CONSTRAINT PK_BTR_HargaType PRIMARY KEY CLUSTERED(HargaTypeId)
)

-- ============================================================
-- File: \BrgContext\BTR_JenisBrg.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_JenisBrg]
(
	JenisBrgId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_JenisBrg_JenisBrgId DEFAULT(''),
	JenisBrgName VARCHAR(15) NOT NULL CONSTRAINT DF_BTR_JenisBrg_JenisBrgName DEFAULT(''),

	CONSTRAINT PK_BTR_JenisBrg PRIMARY KEY CLUSTERED(JenisBrgId)
)


-- ============================================================
-- File: \BrgContext\BTR_Kategori.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_Kategori]
(
	KategoriId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Kategori_KategoriId DEFAULT(''),
	KategoriName VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Kategori_KategoriName DEFAULT(''),
	Code VARCHAR(15) NOT NULL CONSTRAINT DF_BTR_Kategori_Code DEFAULT(''),
	SupplierId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Kategori_SupplierId DEFAULT(''),

	CONSTRAINT PK_BTR_Kategori PRIMARY KEY CLUSTERED(KategoriId)
)


-- ============================================================
-- File: \Finance\BTR_FakturPotBalance.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_FakturPotBalance]
(
	FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FAkturPotBalance_FakturId DEFAULT(''),
	IsHeapFaktur BIT NOT NULL CONSTRAINT DF_BTR_FakturPotBalance_IsHeapFaktur DEFAULT(0),
	NilaiFaktur DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FAkturPotBalance_NilaiFaktur DEFAULT(0),
	NilaiPotong DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FAkturPotBalance_NilaiPotoing DEFAULT(0),
	NilaiSumPost DECIMAL(18,2) NOT NULl CONSTRAINT DF_BTR_FAkturPotBalance_NilaiSumPost DEFAULT(0),

	CONSTRAINT PK_BTR_FakturPotBalance PRIMARY KEY CLUSTERED(FakturId)
)


-- ============================================================
-- File: \Finance\BTR_FAkturPotBalancePost.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_FakturPotBalancePost]
(
	FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FakturPotBalancePost_FakturId DEFAULT(''),
	NoUrut INT NOT NULL CONSTRAINT DF_BTR_FakturPotBalancePost_NoUrut DEFAULT(0),
	PostDate DATETIME NOT NULL CONSTRAINT DF_BTR_FakturPotBalancePost_PostDate DEFAULT('3000-01-01'),
	UserId VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_FakturPotBalancePost_USerId DEFAULT(''),
	ReturJualId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FakturPotBalancePost_ReturJualId DEFAULT(''),
	NilaiRetur DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturPotBalancePost_NilaiRetur DEFAULT(0),
	NilaiPost DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturPotBalancePost_NilaiPost DEFAULT(0),

	CONSTRAINT PK_BTR_FakturPotBalancePost PRIMARY KEY CLUSTERED (FakturId, NoUrut)
)


-- ============================================================
-- File: \Finance\BTR_FpKeluaran.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_FpKeluaran]
(
	FpKeluaranId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FpKeluaran_FpKeluaranId DEFAULT (''),
	FpKeluaranDate DATETIME NOT NULL CONSTRAINT DF_BTR_FpKeluaran_FpKeluaranDate DEFAULT ('3000-01-01'),
	UserDate DATETIME NOT NULL CONSTRAINT DF_BTR_FpKeluaran_UserDate DEFAULT('3000-01-01'),
	Keterangan VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FpKeluaran_Keterangan DEFAULT (''),
	UserId VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_FpKeluaran_UserId DEFAULT (''),
	FakturCount INT NOT NULL CONSTRAINT DF_BTR_FpKeluaran_FakturCount DEFAULT ((0)),
	TotalPpn DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FpKeluaran_TotalPpn DEFAULT ((0)),

	CONSTRAINT PK_BTR_FpKeluaran PRIMARY KEY CLUSTERED (FpKeluaranId ASC)
)


-- ============================================================
-- File: \Finance\BTR_FpKeluaranBrg.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_FpKeluaranBrg]
(
    FpKeluaranBrgId VARCHAR(21) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_FpKeluaranBrgId DEFAULT(''), 
    FpKeluaranFakturId VARCHAR(17) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_FpKeluaranFakturId DEFAULT(''), 
    FpKeluaranId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_FpKeluaranId DEFAULT(''), 
    FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_FakturId DEFAULT(''), 

    Baris INT NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_Baris DEFAULT(0), 
    BarangJasa VARCHAR(1) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_BarangJasa DEFAULT(''),
    KodeBarangJasa VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_KodeBarangJasa DEFAULT(''), 
    NamaBarangJasa VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_NamaBarangJasa DEFAULT(''), 
    NamaSatuanUkur VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_NamaSatuanUkur DEFAULT(''), 
    HargaSatuan DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_HargaSatuan DEFAULT(0), 
    JumlahBarangJasa INT NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_JumlahBarangJasa DEFAULT(0), 
    TotalDiskon DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_TotalDiskon DEFAULT(0), 
    Dpp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_Dpp DEFAULT(0), 
    DppLain DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_DppLain DEFAULT(0), 
    TarifPpn DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_TarifPpn DEFAULT(0), 
    Ppn DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_Ppn DEFAULT(0), 
    TarifPpnBm DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_TarifPpnBm DEFAULT(0), 
    PpnBm DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FpKeluaranBrg_PpnBm DEFAULT(0),

    CONSTRAINT PK_BTR_FpKeluaranBrg PRIMARY KEY CLUSTERED (FpKeluaranBrgId)
)
GO

CREATE INDEX IX_BTR_FpKeluaranBrg_FpKeluaranId
    ON [dbo].[BTR_FpKeluaranBrg] (FpKeluaranId, FpKeluaranBrgId)
GO



-- ============================================================
-- File: \Finance\BTR_FpKeluaranFaktur.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_FpKeluaranFaktur]
(
    FpKeluaranFakturId VARCHAR(17) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_FpKeluaranFakturId  DEFAULT(''),
    FpKeluaranId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_FpKeluaranId  DEFAULT(''),
    FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_FakturId  DEFAULT(''),
    Baris INT NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_Baris  DEFAULT(0),
    
    TanggalFaktur DATETIME NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_TanggalFaktur  DEFAULT('3000-01-01'),
    JenisFaktur VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_JenisFaktur  DEFAULT(''),
    KodeTransaksi VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_KodeTransaksi  DEFAULT(''),
    
    KeteranganTambahan VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_KeteranganTambahan  DEFAULT(''),
    DokumenPendukung VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_DokumenPendukung  DEFAULT(''),
    Referensi VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_Referensi  DEFAULT(''),
    CapFasilitas VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_CapFasilitas  DEFAULT(''),
    IdTkuPenjual VARCHAR(22) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_IdTkuPenjual  DEFAULT(''),
    
    NpwpNikPembeli VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_NpwpNikPembeli  DEFAULT(''),
    JenisIdPembeli VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_JenisIdPembeli  DEFAULT(''),
    NegaraPembeli VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_NegaraPembeli  DEFAULT(''),
    NomorDokumenPembeli VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_NomorDokumenPembeli  DEFAULT(''),
    
    NamaPembeli VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_NamaPembeli  DEFAULT(''),
    AlamatPembeli VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_AlamatPembeli  DEFAULT(''),
    EmailPembeli VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_EmailPembeli  DEFAULT(''),
    IdTkuPembeli VARCHAR(22) NOT NULL CONSTRAINT DF_BTR_FpKeluaranFaktur_IdTkuPembeli  DEFAULT(''),

    CONSTRAINT PK_BTR_FpKeluaranFaktur PRIMARY KEY (FpKeluaranFakturId),
)
GO

CREATE INDEX IX_BTR_FpKeluaranFaktur_FpKeluaranId ON [dbo].[BTR_FpKeluaranFaktur] (FpKeluaranId, FpKeluaranFakturId)
GO



-- ============================================================
-- File: \Finance\BTR_Piutang.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_Piutang]
(
	PiutangId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Piutang_PiutangId DEFAULT(''),
	PiutangDate DATETIME NOT NULL CONSTRAINT DF_BTR_Piutang_PiutangDate DEFAULT('3000-01-01'),
	DueDate DATETIME NOT NULL CONSTRAINT DF_BTR_Piutang_DueDate DEFAULT('3000-01-01'),
	CustomerId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_Piutang_CustomerId DEFAULT(''),
	StatusPiutang INT NOT NULL CONSTRAINT DF_BTR_Piutang_StatusPiutang DEFAULT(0),

	Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Piutang_Total DEFAULT(0),
	Potongan DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Piutang_Potongan DEFAULT(0),
	Terbayar DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Piutang_Terbayar DEFAULT(0),
	Sisa DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Piutang_Sisa DEFAULT(0),

	CONSTRAINT PK_BTR_Piutang PRIMARY KEY CLUSTERED(PiutangId)
)
GO

CREATE INDEX IX_BTR_Piutang_PiutangDate 
	ON [dbo].[BTR_Piutang](PiutangDate, PiutangId)
	WITH(FILLFACTOR=95)
GO

-- ============================================================
-- File: \Finance\BTR_PiutangElement.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_PiutangElement]
(
	PiutangId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_PiutangElement_PiutangId DEFAULT(''),
	NoUrut INT NOT NULL CONSTRAINT DF_BTR_PiutangELement_NoUrut DEFAULT(0),
    ElementTag INT NOT NULL CONSTRAINT DF_BTR_PiutangElement_ElementTag DEFAULT(0),
	ElementName VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_PiutangElement_ElemantName DEFAULT(''),
	NilaiPlus DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PiutangElement_NilaiPlus DEFAULT(''),
	NilaiMinus DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PiutangElement_NilaiMinus DEFAULT(''),
	ElementDate DATETIME NOT NULL CONSTRAINT DF_BTR_PiutangElement_ElementDate DEFAULT('3000-01-01'),

	CONSTRAINT PK_BTR_PiutangElement PRIMARY KEY CLUSTERED(PiutangId, NoUrut)
)


-- ============================================================
-- File: \Finance\BTR_PiutangLunas.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_PiutangLunas]
(
	PiutangId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_PiutangLunas_PiutangId DEFAULT(''),
	NoUrut INT NOT NULL CONSTRAINT DF_BTR_PiutangLunas_NoUrut DEFAULT(0),
	PelunasanId VARCHAR(17) NOT NULL CONSTRAINT DF_BTR_PiutangLunas_PelunasanId DEFAULT(''),
	LunasDate DATETIME NOT NULL CONSTRAINT DF_BTR_PiutangLunas_LunasDate DEFAULT('3000-01-01'),
	TagihanId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_PiutangLunas_TagihanId DEFAULT(''),
	Nilai DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PiutangLunas_Nilai DEFAULT(0),
	JenisLunas INT NOT NULL CONSTRAINT DF_BTR_PiutangLunas_JenisLunas DEFAULT(0),
	JatuhTempoBg DATETIME NOT NULL CONSTRAINT DF_BTR_PiutangLunas_JatuhTempoBg DEFAULT('3000-01-01'),
	NoRekBg VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_PiutangLunas_NoRekBg DEFAULT(''),
	NamaBank VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_PiutangLunas_NamaBank DEFAULT(''),
	AtasNamaBank VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_PiutangLunas_AtasNamaBank DEFAULT(''),

	CONSTRAINT PK_BTR_PiutangLunas PRIMARY KEY CLUSTERED(PiutangId, NoUrut)
)
GO

CREATE INDEX IX_BTR_PiutangLunas_LunasDate 
	ON [dbo].[BTR_PiutangLunas](LunasDate, PiutangId, NoUrut)
	WITH(FILLFACTOR=95)
GO

-- ============================================================
-- File: \Finance\BTR_ReturBalance.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_ReturBalance]
(
	ReturJualId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_ReturBalance_ReturJualId DEFAULT(''),
	NilaiRetur DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBalance_NilaiRetur DEFAULT(0),
	NilaiSumPost DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBalance_NilaiSumPost DEFAULT(0),

	CONSTRAINT PK_BTR_ReturBalance PRIMARY KEY CLUSTERED (ReturJualId)
)


-- ============================================================
-- File: \Finance\BTR_ReturBalancePost.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_ReturBalancePost]
(
	ReturJualId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_ReturJualBalancePost_ReturJualId DEFAULT(''),
	NoUrut INT NOT NULL CONSTRAINT DF_BTR_ReturJualBalancePost_NoUrut DEFAULT(0),
	PostDate DATETIME NOT NULl CONSTRAINT DF_BTR_ReturJualBalancePost_PostDate DEFAULT('3000-01-01'),
	UserId VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_ReturJualBalancePost_UserId DEFAULT(''),

	FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_ReturJualBalancePost_FakturId DEFAULT(''),
	IsHeapFaktur BIT NOT NULL CONSTRAINT DF_BTR_ReturBalancePost_IsHeapFaktur DEFAULT(''),
	NilaiFaktur DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualBalancePost_NilaiFaktur DEFAULT(0),
	NilaiPotong DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualBalancePost_NilaiPotong DEFAULT(0),
	NilaiPost DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualBalancePost_NilaiPost DEFAULT(0),

	CONSTRAINT PK_BTR_ReturBalancePost PRIMARY KEY CLUSTERED(ReturJualId, NoUrut)
)


-- ============================================================
-- File: \Finance\BTR_Tagihan.sql
-- ============================================================

-- create table based on TagihanModel
CREATE TABLE BTR_Tagihan(
	TagihanId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Tagihan_TagihanId DEFAULT(''),
	TagihanDate DATETIME NOT NULL CONSTRAINT DF_BTR_Tagihan_TagihanDate DEFAULT('300-01-01'),
	SalesPersonId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Tagihan_SalesPersonId DEFAULT(''),
	TotalTagihan VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Tagihan_TotalTagihan DEFAULT(0),
	
	CONSTRAINT PK_BTR_Tagihan PRIMARY KEY CLUSTERED(TagihanId)
)
GO


-- ============================================================
-- File: \Finance\BTR_TagihanFaktur.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_TagihanFaktur]
(
    TagihanId  VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_TagihanId DEFAULT(''),
    NoUrut  INT NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_NoUrut DEFAULT(0),
    FakturId  VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_FakturId DEFAULT(''),
    CustomerId  VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_CustomerId DEFAULT(''),
    NilaiTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_NilaiTotal DEFAULT(0),
    NilaiTerbayar  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_NilaiTerbayar DEFAULT(0),
    NilaiTagih DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_NilaiTagih DEFAULT(0),

    IsTandaTerima BIT NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_IsTandaTerima DEFAULT(0),
    Keterangan VARCHAR(255) NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_Keterangan DEFAULT(''),
    TandaTerimaDate DATETIME NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_TandaTerimaDate DEFAULT('3000-01-01'),

    IsTagihUlang BIT NOT NULL CONSTRAINT DF_BTR_TagihanFaktur_IsTagihUlang DEFAULT(0),

    CONSTRAINT PK_BTR_TagihanFaktur PRIMARY KEY CLUSTERED(TagihanId, NoUrut)
)


-- ============================================================
-- File: \Helper\BTR_Doc.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_Doc]
(
        DocId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Doc_DocId DEFAULT(''),
        Code VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Doc_Code DEFAULT(''),
        DocType VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Doc_DocType DEFAULT(''),
        DocDesc VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Doc_DocDesc DEFAULT(''),
        DocDate DATETIME NOT NULL CONSTRAINT DF_BTR_Doc_DocDate DEFAULT('3000-01-01'),

        WarehouseId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Doc_WarehouseId DEFAULT(''),
        DocPrintStatus INT NOT NULL CONSTRAINT DF_BTR_Doc_DocPrintStatus DEFAULT(0),

        CONSTRAINT PK_BTR_Doc PRIMARY KEY CLUSTERED(DocId)
)


-- ============================================================
-- File: \Helper\BTR_DocAction.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_DocAction]
(
	DocId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_DocAction_DocId DEFAULT(''),
	ActionDate DATETIME NOT NULL CONSTRAINT DF_BTR_DocAction_ActionDate DEFAULT('3000-01-01'),
	[Action] VARCHAR(256) NOT NULL CONSTRAINT DF_BTR_DocAction_Action DEFAULT(''),
)
GO

CREATE CLUSTERED INDEX CX_BTR_DocAction
	ON BTR_DocAction (DocId, ActionDate)
	WITH(FILLFACTOR=80)
GO


-- ============================================================
-- File: \Helper\BTR_Menu.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_Menu]
(
	MenuId VARCHAR(4) NOT NULL CONSTRAINT DF_BTR_Menu_MenuId DEFAULT(''),
	GroupOrder INT NOT NULL CONSTRAINT DF_BTR_Menu_GroupMenu DEFAULT(0),
	FormType VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Menu_FormType DEFAULT(''),
	MenuName VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Menu_MenuName DEFAULT(''),
	Caption VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_Menu_Caption DEFAULT(''),

	CONSTRAINT PK_BTR_Menu PRIMARY KEY CLUSTERED (MenuId)
)
GO

CREATE INDEX IX_BTR_Menu_GroupMenu
	ON BTR_Menu (GroupOrder, MenuId)
GO


-- ============================================================
-- File: \Helper\BTR_ParamNo.sql
-- ============================================================

CREATE TABLE BTR_ParamNo(
    Prefix VARCHAR(10) NOT NULL CONSTRAINT BTR_ParamNo_Prefix DEFAULT(''),
    HexVal VARCHAR(20) NOT NULL CONSTRAINT BTR_ParamNo_HexVal DEFAULT(''),
    
    CONSTRAINT PK_BTR_ParamNo PRIMARY KEY CLUSTERED(Prefix)
)

-- ============================================================
-- File: \Helper\BTR_ParamSistem.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_ParamSistem]
(
	ParamCode VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_ParamSistem_ParamCode DEFAULT (''),
	ParamValue VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_ParamSistem_ParamValue DEFAULT (''),

	CONSTRAINT PK_BTR_ParamSistem PRIMARY KEY (ParamCode)
)


-- ============================================================
-- File: \Helper\BTR_Role.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_Role]
(
	RoleId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Role_RoleId DEFAULT(''),
	RoleName VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Role_RoleName DEFAULT(''),

	CONSTRAINT PK_BTR_Role PRIMARY KEY CLUSTERED(RoleId)
)

-- ============================================================
-- File: \Helper\BTR_RoleMenu.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_RoleMenu]
(
	RoleId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_RoleMenu_RoleId DEFAULT(''),
	MenuId VARCHAR(4) NOT NULL CONSTRAINT DF_BTR_RoleMenu_MenuId DEFAULT(''),

	CONSTRAINT PK_BTR_RoleMenu PRIMARY KEY CLUSTERED(RoleId, MenuId)
)

-- ============================================================
-- File: \Helper\BTR_User.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_User]
(
	UserId VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_User_UserId DEFAULT(''),
	UserName VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_User_UserName DEFAULT(''),
	Password VARCHAR(64) NOT NULL CONSTRAINT DF_BTR_User_Password DEFAULT(''),
	Prefix VARCHAR(2) NOT NULL CONSTRAINT DF_BTR_User_Prefix DEFAULT(''),
	RoleId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_User_Role DEFAULT(''),
	Email VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_User_Email DEFAULT(''),

	CONSTRAINT PK_BTR_User PRIMARY KEY CLUSTERED(UserId)
)
GO

CREATE UNIQUE INDEX UX_BTR_User_Email
	ON BTR_User (Email)
	WHERE Email <> ''
GO


-- ============================================================
-- File: \Helper\BTR_UserParam.sql
-- ============================================================

CREATE TABLE BTR_UserParam(
    UserId VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_UserParam_UserId DEFAULT(''),
    ParamKey VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_UserParam_ParamKey DEFAULT(''),
    ParamVal VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_UserParam_ParamVal DEFAULT(''),
    
    CONSTRAINT PK_BTR_UserParam PRIMARY KEY CLUSTERED(UserId, ParamKey)   
)



-- ============================================================
-- File: \InventoryContext\BTR_Adjustment.sql
-- ============================================================

-- create table BTR_Adjustment based on AdjustmentModel
CREATE TABLE BTR_Adjustment(
    AdjustmentId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Adjustment_AdjustmentId DEFAULT(''),
    AdjustmentDate DATE NOT NULL CONSTRAINT DF_BTR_Adjustment_AdjustmentDate DEFAULT('3000-01-01'),
    WarehouseId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Adjustment_WarehouseId DEFAULT(''),
    Alasan VARCHAR(255) NOT NULL CONSTRAINT DF_BTR_Adjustment_Alasan DEFAULT(''),
    
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_Adjustment_BrgId DEFAULT(''),
    QtyAwalBesar INT NOT NULL CONSTRAINT DF_BTR_Adjustment_QtyAwalBesar DEFAULT(0),
    QtyAwalKecil INT NOT NULL CONSTRAINT DF_BTR_Adjustment_QtyAwalKecil DEFAULT(0),
    QtyAwalInPcs INT NOT NULL CONSTRAINT DF_BTR_Adjustment_QtyAwalInPcs DEFAULT(0),
    
    QtyAdjustBesar INT NOT NULL CONSTRAINT DF_BTR_Adjustment_QtyAdjustBesar DEFAULT(0),
    QtyAdjustKecil INT NOT NULL CONSTRAINT DF_BTR_Adjustment_QtyAdjustKecil DEFAULT(0),
    QtyAdjustInPcs INT NOT NULL CONSTRAINT DF_BTR_Adjustment_QtyAdjustInPcs DEFAULT(0),

    QtyAkhirBesar INT NOT NULL CONSTRAINT DF_BTR_Adjustment_QtyAkhirBesar DEFAULT(0),
    QtyAkhirKecil INT NOT NULL CONSTRAINT DF_BTR_Adjustment_QtyAkhirKecil DEFAULT(0),
    QtyAkhirInPcs INT NOT NULL CONSTRAINT DF_BTR_Adjustment_QtyAkhirInPcs DEFAULT(0),
    
    CONSTRAINT PK_BTR_Adjustment PRIMARY KEY CLUSTERED (AdjustmentId)
)

-- ============================================================
-- File: \InventoryContext\BTR_Depo.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_Depo]
(
	DepoId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_Depo_DepoId DEFAULT(''),
	DepoName VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Depo_DepoName DEFAULT(''),
	CONSTRAINT PK_BTR_Depo PRIMARY KEY CLUSTERED (DepoId)
)
GO


-- ============================================================
-- File: \InventoryContext\BTR_Driver.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_Driver]
(
	DriverId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Driver_DriverId DEFAULT(''),
	DriverName VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Driver_DriverName DEFAULT(''),
	IsAktif BIT NOT NULL CONSTRAINT DF_BTR_Driver_IsAKtif DEFAULT(1),

	CONSTRAINT PK_BTR_Driver PRIMARY KEY CLUSTERED (DriverId)
)


-- ============================================================
-- File: \InventoryContext\BTR_ImportOpname.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_ImportOpname]
(
	BrgCode VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_ImportOpname_BrgId DEFAULT(''),
	WarehouseId VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_ImportOpname_WarehouseId DEFAULT(''),
	Qty VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_ImportOpname_Qty DEFAULT(0),

	CONSTRAINT PK_BTR_ImportOpname PRIMARY KEY CLUSTERED(BrgCode, WarehouseId)
)


-- ============================================================
-- File: \InventoryContext\BTR_Mutasi.sql
-- ============================================================

CREATE TABLE BTR_Mutasi(
    MutasiId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Mutasi_MutasiId DEFAULT(''), 
    MutasiDate DATETIME NOT NULL CONSTRAINT DF_BTR_Mutasi_MutasiDate DEFAULT('3000-01-01'), 
    KlaimDate DATETIME NOT NULL CONSTRAINT DF_BTR_Mutasi_KlaimDate DEFAULT('3000-01-01'),
    JenisMutasi INT NOT NULL CONSTRAINT DF_BTR_Mutasi_JenisMutasi DEFAULT(0), 
    WarehouseId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_Mutasi_WarehouseId DEFAULT(''),
    Keterangan VARCHAR(128) NOT NULL CONSTRAINT DF_BTR_Mutasi_Keterangan DEFAULT(''), 
    NilaiSediaan DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Mutasi_NilaiSediaan DEFAULT(0),
    
    UserId VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Mutasi_UserId DEFAULT(''), 
    CreateTime DATETIME NOT NULL CONSTRAINT DF_BTR_Mutasi_CreateTime DEFAULT('3000-01-01'), 
    LastUpdate DATETIME NOT NULL CONSTRAINT DF_BTR_Mutasi_LastUpdate DEFAULT('3000-01-01'), 
    VoidDate DATETIME NOT NULL CONSTRAINT DF_BTR_Mutasi_VoidDate DEFAULT('3000-01-01'), 
    UserIdVoid VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Mutasi_UserIdVoid DEFAULT(''),
    
    CONSTRAINT PK_BTR_Mutasi PRIMARY KEY CLUSTERED(MutasiId)   
)

-- ============================================================
-- File: \InventoryContext\BTR_MutasiDisc.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_MutasiDisc]
(
	MutasiId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_MutasiDisc_MutasiId DEFAULT(''),
	MutasiItemId VARCHAR(16) NOT NULL CONSTRAINT DF_BTR_MutasiDisc_MutasiItemId DEFAULT(''),
	MutasiDiscId VARCHAR(19) NOT NULL CONSTRAINT DF_BTR_MutasiDisc_MutasiDiscId DEFAULT(''),
	NoUrut INT NOT NULL CONSTRAINT DF_BTR_MutasiDisc_NoUrut DEFAULT(0),
	BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_MutasiDisc_BrgId DEFAULT(''),
	DiscProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_MutasiDisc_DiscProsen DEFAULT(0),
	DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_MutasiDisc_DiscRp DEFAULT(0),

	CONSTRAINT PK_BTR_MutasiDisc PRIMARY KEY CLUSTERED (MutasiDiscId)	
)
GO

CREATE INDEX IX_BTR_MutasiDisc_MutasiId
	ON BTR_MutasiDisc (MutasiId, MutasiDiscId)
GO



-- ============================================================
-- File: \InventoryContext\BTR_MutasiItem.sql
-- ============================================================

CREATE TABLE BTR_MutasiItem(
    MutasiId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_MutasiItem_MutasiId DEFAULT(''),
    MutasiItemId VARCHAR(16) NOT NULL CONSTRAINT DF_BTR_MutasiItem_MutasiItemId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_MutasiItem_NoUrut DEFAULT(0),
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_MutasiItem_BrgId DEFAULT(''),
    
    QtyInputStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_MutasiItem_QtyInputStr DEFAULT(''),
    QtyBesar INT NOT NULL CONSTRAINT DF_BTR_MutasiItem_QtyBesar DEFAULT(0),
    SatBesar VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_MutasiItem_SatBesar DEFAULT(''),
    Conversion INT NOT NULL CONSTRAINT DF_BTR_MutasiItem_Conversion DEFAULT(0),
    HppBesar DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_MutasiItem_HppBesar DEFAULT(0),
    
    QtyKecil INT NOT NULL CONSTRAINT DF_BTR_MutasiItem_QtyKecil DEFAULT(0),
    SatKecil VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_MutasiItem_SatKecil DEFAULT(''),
    HppKecil DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_MutasiItem_HppKecil DEFAULT(0),
    
    StokBesar INT NOT NULL CONSTRAINT DF_BTR_MutasiItem_StokBesar DEFAULT(0),
    StokKecil INT NOT NULL CONSTRAINT DF_BTR_MutasiItem_StokKecil DEFAULT(0),
    
    Qty INT NOT NULL CONSTRAINT DF_BTR_MutasiItem_Qty DEFAULT(0),
    Sat VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_MutasiItem_Sat DEFAULT(''),
    Hpp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_MutasiItem_Hpp DEFAULT(0),
    
    DiscInputStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_MutasiItem_DiscInputStr DEFAULT(''),
    DiscDetilStr VARCHAR(64) NOT NULL CONSTRAINT DF_BTR_MutasiItem_DiscDetilStr DEFAULT(''),
    DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_MutasiItem_DiscRp DEFAULT(0),

    QtyDetilStr VARCHAR(64) NOT NULL CONSTRAINT DF_BTR_MutasiItem_QtyDetilStr DEFAULT(''),
    StokDetilStr VARCHAR(64) NOT NULL CONSTRAINT DF_BTR_MutasiItem_StokDetilStr DEFAULT(''),
    HppDetilStr VARCHAR(64) NOT NULL CONSTRAINT DF_BTR_MutasiItem_HppDetilStr DEFAULT(''),
    NilaiSediaan DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_MutasiItem_NilaiSediaan DEFAULT(0),
    
    CONSTRAINT PK_BTR_MutasiItem PRIMARY KEY CLUSTERED(MutasiItemId),
)
GO

CREATE INDEX IX_BTR_MutasiItem_MutasiId
    ON BTR_MutasiItem(MutasiId, MutasiItemId)
    WITH(FILLFACTOR=95)
GO

-- ============================================================
-- File: \InventoryContext\BTR_Opaname.sql
-- ============================================================

CREATE TABLE BTR_Opname(
    OpnameId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Opname_OpanameId DEFAULT(''),
    OpnameDate DATETIME NOT NULL CONSTRAINT DF_BTR_Opname_OpanameDate DEFAULT(''),
    UserId VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Opname_UserId DEFAULT(''),
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_Opname_BrgId DEFAULT(''),
    BrgCode VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Opname_BrgCode DEFAULT(''),
    WarehouseId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_Opname_WarehouseId DEFAULT(''),
    
    Qty2Awal INT NOT NULL CONSTRAINT DF_BTR_Opname_Qty2Awal DEFAULT(0),
    Qty2Opname INT NOT NULL CONSTRAINT DF_BTR_Opname_Qty2Opname DEFAULT(0),
    Qty2Adjust INT NOT NULL CONSTRAINT DF_BTR_Opname_Qty2Adjust DEFAULT(0),
    Satuan2 VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_Opname_Satuan2 DEFAULT(''),

    Qty1Awal INT NOT NULL CONSTRAINT DF_BTR_Opname_Qty1Awal DEFAULT(0),
    Qty1Opname INT NOT NULL CONSTRAINT DF_BTR_Opname_Qty1Opname DEFAULT(0),
    Qty1Adjust INT NOT NULL CONSTRAINT DF_BTR_Opname_Qty1Adjust DEFAULT(0),
    Satuan1 VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_Opname_Satuan1 DEFAULT(''),
    
    Nilai DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Opname_Nilai DEFAULT(0),                    
)

-- ============================================================
-- File: \InventoryContext\BTR_Packing.sql
-- ============================================================

CREATE  TABLE [dbo].[BTR_Packing]
(
	PackingId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Packing_PackingId DEFAULT(''),
	PackingDate DATETIME NOT NULL CONSTRAINT DF_BTR_Packing_PackingDate DEFAULT(''),
	
	WarehouseId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Packing_WarehouseId DEFAULT(''),
	DriverId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Packing_DriverId DEFAULT(''),
	DeliveryDate DATETIME NOT NULL CONSTRAINT DF_BTR_Packing_DeliveryDate DEFAULT('3000-01-01'),
    
    TglAwalFaktur DATETIME NOT NULL CONSTRAINT DF_BTR_Packing_TglAwalFaktur DEFAULT('3000-01-01'),
    TglAKhirFaktur DATETIME NOT NULL CONSTRAINT DF_BTR_Packing_TglAKhirFaktur DEFAULT('3000-01-01'),
    KeywordSearch VARCHAR NOT NULL CONSTRAINT DF_BTR_Packing_KeywordSearch DEFAULT('')

	CONSTRAINT PK_BTR_Packing PRIMARY KEY CLUSTERED (PackingId)
)
GO

CREATE INDEX IX_BTR_Packing_DeliveryDate
	ON BTR_Packing (DeliveryDate, PackingId)
	WITH(FILLFACTOR=90)
GO

-- ============================================================
-- File: \InventoryContext\BTR_PackingBrg.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_PackingBrg]
(
        PackingId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_PackingBrg_PackingId DEFAULT(''),
        FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_PackingBrg_FakturId DEFAULT(''),
        SupplierId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_PackingBrg_SupplierId DEFAULT(''),
        BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_PackingBrg_BrgId DEFAULT(''),
        QtyBesar INT NOT NULL CONSTRAINT DF_BTR_PackingBrg_QtyBesar DEFAULT(0),
        SatBesar VARCHAR(15) NOT NULL CONSTRAINT DF_BTR_PackingBrg_SatBesar DEFAULT(''),
        QtyKecil INT NOT NULL CONSTRAINT DF_BTR_PackingBrg_QtyKecil DEFAULT(0),
        SatKecil VARCHAR(15) NOT NULL CONSTRAINT DF_BTR_PackingBrg_SatKecil DEFAULT(''),
        HargaJual DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PackingBrg_HargaJual DEFAULT(0),

        CONSTRAINT PK_BTR_PackingBrg PRIMARY KEY CLUSTERED(PackingId, SupplierId, BrgId, FakturId)
)


-- ============================================================
-- File: \InventoryContext\BTR_PackingFaktur.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_PackingFaktur]
(
	PackingId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_PackingFaktur_PackingId DEFAULT(''),
	NoUrut INT NOT NULL CONSTRAINT DF_BTR_PackingFaktur_NoUrut DEFAULT(0),
	FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_PackingFaktur_FakturId DEFAULT(''),

	CONSTRAINT PK_BTR_PackingFaktur PRIMARY KEY CLUSTERED(PackingId, FakturId)
)
GO

CREATE UNIQUE INDEX UX_BTR_PackingFaktur_PackingIdFakturId
	ON BTR_PackingFaktur (PackingId, FakturId)
	WITH(FILLFACTOR=90)
GO

CREATE INDEX IX_BTR_PackingFaktur_FakturId
	ON BTR_PackingFaktur (FakturId, PackingId)
	WITH(FILLFACTOR=90)
GO


-- ============================================================
-- File: \InventoryContext\BTR_ReturJual.sql
-- ============================================================

-- create table for ReturJualModel
CREATE TABLE [dbo].[BTR_ReturJual](
	ReturJualId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_ReturJual_ReturJualId DEFAULT(''),
	ReturJualDate DATETIME NOT NULL CONSTRAINT DF_BTR_ReturJual_ReturJualDate DEFAULT(''),
	JenisRetur VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_ReturJual_JenisRetur DEFAULT(''),
	ReturJualCode VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_ReturJual_ReturJualCode DEFAULT(''),

	CustomerId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_ReturJual_CustomerId DEFAULT(''),
	WarehouseId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_ReturJual_WarehouseId DEFAULT(''),
    SalesPersonId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_ReturJual_SalesPersonId DEFAULT(''),
    DriverId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_ReturJual_DriverId DEFAULT(''),
	UserId VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_ReturJual_UserId DEFAULT(''),
	Note VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_ReturJual_Note DEFAULT(''),
	Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJual_Total DEFAULT(''),
	DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJual_DiscRp DEFAULT(''),
	PpnRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJual_PpnRp DEFAULT(''),
	GrandTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJual_GrandTotal DEFAULT(''),

    VoidDate DATETIME NOT NULL CONSTRAINT DF_BTR_ReturJual_VoidDate DEFAULT('3000-01-01'),
    UserIdVoid VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_ReturJual_UserIdVoid DEFAULT(''),
	
	CONSTRAINT [PK_BTR_ReturJual] PRIMARY KEY CLUSTERED  (ReturJualId)
)

-- ============================================================
-- File: \InventoryContext\BTR_ReturJualItem.sql
-- ============================================================

-- create table for ReturJualItemModel
CREATE TABLE BTR_ReturJualItem(
    ReturJualId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_ReturJualId DEFAULT(''),
    ReturJualItemId VARCHAR(16) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_ReturJualItemId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_ReturJualItem_NoUrut DEFAULT(0),
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_BrgId DEFAULT(''),
    BrgCode VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_BrgCode DEFAULT(''),

    QtyInputStr VARCHAR(128) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_QtyInputStr DEFAULT(''),
    HrgInputStr VARCHAR(128) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_HrgInputStr DEFAULT(''),
    QtyHrgDetilStr VARCHAR(128) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_QtyHrgDetilStr DEFAULT(''),
    DiscInputStr VARCHAR(128) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_DiscInputStr DEFAULT(''),
    DiscDetilStr VARCHAR(128) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_DiscDetilStr DEFAULT(''),    
    
    SubQty INT NOT NULL CONSTRAINT DF_BTR_ReturJualItem_QtySub DEFAULT(0),
    SubSatuan VARCHAR(15) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_SubUnit DEFAULT(''),

    Qty INT NOT NULL CONSTRAINT DF_BTR_ReturJualItem_Qty DEFAULT(0),
    HrgSat DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_HrgSat DEFAULT(0),
    SubTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_SubTotal DEFAULT(0),
    DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_DiscRp DEFAULT(0),
    PpnRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_PpnRp DEFAULT(0),
    PpnProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_PpnProsen DEFAULT(0),
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualItem_Total DEFAULT(0),

    CONSTRAINT PK_BTR_ReturJualItem PRIMARY KEY CLUSTERED(ReturJualItemId),
)

-- ============================================================
-- File: \InventoryContext\BTR_ReturJualItemDisc.sql
-- ============================================================

-- create table for ReturJualItemDiscModel
CREATE TABLE BTR_ReturJualItemDisc(
	ReturJualId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_ReturJualItemDisc_ReturJualId DEFAULT(''),
	ReturJualItemId VARCHAR(16) NOT NULL CONSTRAINT DF_BTR_ReturJualItemDisc_ReturJualItemId DEFAULT(''),
	ReturJualItemDiscId VARCHAR(18) NOT NULL CONSTRAINT DF_BTR_ReturJualItemDisc_ReturJualItemDiscId DEFAULT(''),
	NoUrut INT NOT NULL CONSTRAINT DF_BTR_ReturJualItemDisc_NoUrut DEFAULT(0),
	BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_ReturJualItemDisc_BrgId DEFAULT(''),
	
	BaseHrg DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualItemDisc_BaseHrg DEFAULT(0),
	DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualItemDisc_DiscRp DEFAULT(0),
	DiscProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualItemDisc_DiscProsen DEFAULT(0),

	CONSTRAINT PK_BTR_ReturJualItemDisc PRIMARY KEY CLUSTERED(ReturJualItemDiscId)
)

-- ============================================================
-- File: \InventoryContext\BTR_ReturJualItemQtyHrg.sql
-- ============================================================

-- create table for RetyrJualItemQtyHrgModel
CREATE TABLE BTR_ReturJualItemQtyHrg(
	ReturJualId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_ReturJualQtyHrg_ReturJualId DEFAULT(''),
	ReturJualItemId VARCHAR(16) NOT NULL CONSTRAINT DF_BTR_ReturJualQtyHrg_ReturJualItemId DEFAULT(''),
	ReturJualItemQtyHrgId VARCHAR(16) NOT NULL CONSTRAINT DF_BTR_ReturJualQtyHrg_ReturJualItemQtyHrgId DEFAULT(''),
	NoUrut INT NOT NULL CONSTRAINT DF_BTR_ReturJualQtyHrg_NoUrut DEFAULT(0),
	Qty INT NOT NULL CONSTRAINT DF_BTR_ReturJualQtyHrg_Qty DEFAULT(0),
	HrgSat DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualQtyHrg_HrgSat DEFAULT(0),
	SubTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualQtyHrg_SubTotal DEFAULT(0),
	DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualQtyHrg_DiscRp DEFAULT(0),
	PpnRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualQtyHrg_PpnRp DEFAULT(0),
	Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturJualQtyHrg_Total DEFAULT(0),

	CONSTRAINT PK_BTR_ReturJualQtyHrg PRIMARY KEY CLUSTERED(ReturJualItemId)
)




-- ============================================================
-- File: \InventoryContext\BTR_ReturnOrder.sql
-- ============================================================

-- create table for ReturnOrderModel
CREATE TABLE BTR_ReturnOrder(
    ReturnOrderId   VARCHAR(26)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ReturnOrderId   DEFAULT(''),
    ReturnOrderNo   VARCHAR(20)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ReturnOrderNo   DEFAULT(''),
    ReturnOrderDate DATETIME     NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ReturnOrderDate DEFAULT('3000-01-01'),
    WarehouseCode   VARCHAR(20)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_WarehouseCode   DEFAULT(''),
    CustomerId      VARCHAR(6)   NOT NULL CONSTRAINT DF_BTR_ReturnOrder_CustomerId      DEFAULT(''),
    SalesPersonId   VARCHAR(5)   NOT NULL CONSTRAINT DF_BTR_ReturnOrder_SalesPersonId   DEFAULT(''),
    DriverId        VARCHAR(5)   NOT NULL CONSTRAINT DF_BTR_ReturnOrder_DriverId        DEFAULT(''),
    Note            VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_ReturnOrder_Note            DEFAULT(''),
    Status          VARCHAR(10)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_Status          DEFAULT('Synced'),

    CreatedBy       VARCHAR(50)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_CreatedBy       DEFAULT(''),
    CreatedDate     DATETIME     NOT NULL CONSTRAINT DF_BTR_ReturnOrder_CreatedDate     DEFAULT('3000-01-01'),
    ModifiedBy      VARCHAR(50)  NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ModifiedBy      DEFAULT(''),
    ModifiedDate    DATETIME     NOT NULL CONSTRAINT DF_BTR_ReturnOrder_ModifiedDate    DEFAULT('3000-01-01'),

    RowVer          ROWVERSION   NOT NULL,

    CONSTRAINT PK_BTR_ReturnOrder PRIMARY KEY CLUSTERED (ReturnOrderId)
)
GO

CREATE INDEX IX_BTR_ReturnOrder_Status
    ON BTR_ReturnOrder(Status, ReturnOrderDate)
    WITH(FILLFACTOR=75)
GO

CREATE INDEX IX_BTR_ReturnOrder_CustomerId
    ON BTR_ReturnOrder(CustomerId, ReturnOrderId)
    WITH(FILLFACTOR=75)
GO


-- ============================================================
-- File: \InventoryContext\BTR_ReturnOrderItem.sql
-- ============================================================

-- create table for ReturnOrderItemModel
CREATE TABLE BTR_ReturnOrderItem(
    ReturnOrderId VARCHAR(26)     NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_ReturnOrderId DEFAULT(''),
    NoUrut        INT             NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_NoUrut        DEFAULT(0),
    BrgId         VARCHAR(6)      NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_BrgId         DEFAULT(''),
    BrgCode       VARCHAR(20)     NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_BrgCode       DEFAULT(''),
    Qty           DECIMAL(18,2)   NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_Qty           DEFAULT(0),
    SatId         VARCHAR(7)      NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_SatId         DEFAULT(''),
    JenisRetur    VARCHAR(5)      NOT NULL CONSTRAINT DF_BTR_ReturnOrderItem_JenisRetur    DEFAULT(''),

    CONSTRAINT PK_BTR_ReturnOrderItem PRIMARY KEY CLUSTERED (ReturnOrderId, NoUrut)
)
GO


-- ============================================================
-- File: \InventoryContext\BTR_Stok.sql
-- ============================================================

CREATE TABLE BTR_Stok(
    StokId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Stok_StokId DEFAULT(''),
    StokDate DATETIME NOT NULL CONSTRAINT DF_BTR_Stok_StokDate DEFAULT('3000-01-01'),
    ReffId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Stok_ReffId DEFAULT(''),

    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_Stok_BrgId DEFAULT(''),
    WarehouseId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Stok_WarehouseId DEFAULT(''),
    
    QtyIn INT NOT NULL CONSTRAINT DF_BTR_Stok_QtyIn DEFAULT(0),
    Qty INT NOT NULL CONSTRAINT DF_BTR_Stok_Qty DEFAULT(0),
    NilaiPersediaan DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Stok_NilaiPersediaan DEFAULT(0),
    
    CONSTRAINT PK_BTR_Stok PRIMARY KEY CLUSTERED (StokId)
)
GO

CREATE INDEX IX_BTR_Stok_BrgId_WarehouseId
    ON BTR_Stok(BrgId, WarehouseId, Qty, StokId)
    WITH(FILLFACTOR=60)
GO

CREATE INDEX IX_BTR_Stok_ReffId
    ON BTR_Stok(ReffId, StokId)
    WITH(FILLFACTOR=60)
GO


-- ============================================================
-- File: \InventoryContext\BTR_StokBalanceWarehouse.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_StokBalanceWarehouse]
(
	BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_StokBalance_BrgId DEFAULT(''),
	WarehouseId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_StokBalance_WarehouseId DEFAULT(''),
	Qty INT NOT NULL CONSTRAINT DF_BTR_StokBalance_Qty DEFAULT(0),

	CONSTRAINT PK_BTR_StokBalanceWarehouse PRIMARY KEY CLUSTERED(BrgId, WarehouseId)
)
GO

CREATE INDEX IX_BTR_StokBalanceWarehouse_WarehouseId 
    ON [dbo].[BTR_StokBalanceWarehouse](WarehouseId, BrgId)
GO


-- ============================================================
-- File: \InventoryContext\BTR_StokMutasi.sql
-- ============================================================

CREATE TABLE BTR_StokMutasi(
    StokId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_StokMutasi_StokId DEFAULT(''),
    StokMutasiId VARCHAR(18) NOT NULL CONSTRAINT DF_BTR_StokMutasi_StokMutasiId DEFAULT(''),
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_StokMutasi_BrgId  DEFAULT(''),
    WarehouseId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_StokMutasi_WarehouseId DEFAULT(''),

    ReffId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_StokMutasi_ReffId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_StokMutasi_NoUrut DEFAULT(0),
    JenisMutasi VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_StokMutasi_JenisMutasi DEFAULT(''),
    MutasiDate DATETIME NOT NULL CONSTRAINT DF_BTR_StokMutasi_MutasiDate DEFAULT('3000-01-01'),
    PencatatanDate DATETIME NOT NULL CONSTRAINT DF_BTR_StokMutasi_PencatatanDate DEFAULT('3000-01-01'),

    QtyIn INT NOT NULL CONSTRAINT DF_BTR_StokMutasi_QtyIn DEFAULT(0),
    QtyOut INT NOT NULL CONSTRAINT DF_BTR_StokMutasi_QtyOut DEFAULT(0),
    HargaJual DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_StokMutasi_HargaJual DEFAULT(0),
    Keterangan VARCHAR(128) NOT NULL CONSTRAINT DF_BTR_StokMutasi_Keterangan DEFAULT(''),
    
    CONSTRAINT PK_BTR_StokMutasi PRIMARY KEY CLUSTERED (StokMutasiId)
)
GO

CREATE INDEX IX_BTR_StokMutasi_StokId
    ON BTR_StokMutasi (StokId, StokMutasiId)
    WITH(FILLFACTOR=95)                                   
GO


CREATE INDEX IX_BTR_StokMutasi_ReffId
    ON BTR_StokMutasi (ReffId, StokMutasiId)
    WITH(FILLFACTOR=95)
GO


-- ============================================================
-- File: \InventoryContext\BTR_StokOp.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_StokOp]
(
	StokOpId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_StokOp_StokOpId DEFAULT(''),
	StokOpDate DATETIME NOT NULL CONSTRAINT DF_BTR_StokOp_StokOpDate DEFAULT('3000-01-01'),
	PeriodeOp DATETIME NOT NULL CONSTRAINT DF_BTR_StokOp_PeriodeOp DEFAULT('3000-01-01'),
	BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_StokOp_BrgId DEFAULT(''),
	WarehouseId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_StokOp_WarehouseId DEFAULT(''),
	
	QtyBesarAwal INT NOT NULL CONSTRAINT DF_BTR_StokOp_QtyBesarAwal DEFAULT(0),
	QtyKecilAwal INT NOT NULL CONSTRAINT DF_BTR_StokOp_QtyKecilAwal DEFAULT(0),
	QtyPcsAwal INT NOT NULL CONSTRAINT DF_BTR_StokOp_QtyPcsAwal     DEFAULT(0),
	
	QtyBesarOpname INT NOT NULL CONSTRAINT DF_BTR_StokOp_QtyBesarOpname DEFAULT(0),
	QtyKecilOpname INT NOT NULL CONSTRAINT DF_BTR_StokOp_QtyKecilOpname DEFAULT(0),
	QtyPcsOpname INT NOT NULL CONSTRAINT DF_BTR_StokOp_QtyPcsOpname DEFAULT(0),
	
	QtyBesarAdjust INT NOT NULL CONSTRAINT DF_BTR_StokOp_QtyBesarAdjust DEFAULT(0),
	QtyKecilAdjust INT NOT NULL CONSTRAINT DF_BTR_StokOp_QtyKecilAdjust DEFAULT(0),
	QtyPcsAdjust INT NOT NULL CONSTRAINT DF_BTR_StokOp_QtyPcsAdjust DEFAULT(0),

    QtyOpnameInputStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_StokOp_QtyOpnameInputStr DEFAULT(''),
	UserId VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_StokOp_UserId DEFAULT(''),

	CONSTRAINT PK_BTR_StokOp PRIMARY KEY CLUSTERED (StokOpId)
)


-- ============================================================
-- File: \InventoryContext\BTR_Warehouse.sql
-- ============================================================

CREATE TABLE BTR_Warehouse(
    WarehouseId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Warehouse_WarehouseId DEFAULT(''),
    WarehouseName VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Warehouse_WarehouseName DEFAULT(''),
    IsSpecial BIT NOT NULL CONSTRAINT DF_BTR_Warehouse_IsSpecial DEFAULT(0),
    IsAktif BIT NOT NULL CONSTRAINT DF_BTR_Warehouse_IsAktif DEFAULT(1),

    CONSTRAINT PK_BTR_Warehouse PRIMARY KEY CLUSTERED(WarehouseId)
)


-- ============================================================
-- File: \PurchaseContext\BTR_Invoice.sql
-- ============================================================

CREATE TABLE BTR_Invoice
(
    InvoiceId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Invoice_InvoiceId DEFAULT(''),    
    InvoiceDate DATETIME NOT NULL CONSTRAINT DF_BTR_Invoice_InvoiceDate DEFAULT('3000-01-01'), 
    InvoiceCode VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Invoice_InvoiceCode DEFAULT(''), 

    SupplierId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Invoice_SupplierId DEFAULT(''), 
    WarehouseId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Invoice_WarehouseId DEFAULT(''), 

    NoFakturPajak VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Invoice_NoFakturPajak DEFAULT(''), 
    DueDate DATETIME NOT NULL CONSTRAINT DF_BTR_Invoice_DueDate DEFAULT('3000-01-01'), 
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Invoice_Total DEFAULT(0), 
    Disc DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Invoice_Discount DEFAULT(0),
    Dpp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Invoice_Dpp DEFAULT(0),
    Tax DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Invoice_Tax DEFAULT(0), 
    GrandTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Invoice_GrandTotal DEFAULT(''),

    CreateTime DATETIME NOT NULL CONSTRAINT DF_BTR_Invoice_CreateTime DEFAULT('3000-01-01'),
    LastUpdate DATETIME NOT NULL CONSTRAINT DF_BTR_Invoice_LastUpdate DEFAULT('3000-01-01'),
    UserId VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Invoice_UserId DEFAULT(''),

    VoidDate DATETIME NOT NULL CONSTRAINT DF_BTR_Invoice_VoidDate DEFAULT('3000-01-01'),
    UserIdVoid VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Invoice_UserIdVoid DEFAULT(''),

    
    CONSTRAINT PK_BTR_Invoice PRIMARY KEY CLUSTERED (InvoiceId)
)

-- ============================================================
-- File: \PurchaseContext\BTR_InvoiceDisc.sql
-- ============================================================

CREATE TABLE BTR_InvoiceDisc
(
    InvoiceId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_InvoiceDisc_DiscId DEFAULT(''),
    InvoiceItemId VARCHAR(17) NOT NULL CONSTRAINT DF_BTR_InvoiceDisc_DiscItemId DEFAULT(''),
    InvoiceDiscId VARCHAR(19) NOT NULL CONSTRAINT DF_BTR_InvoiceDisc_DiscDiscId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_InvoiceDisc_NoUrut DEFAULT(0),
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_InvoiceDisc_BrgId DEFAULT(''),
    DiscProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceDisc_DiscProsen DEFAULT(0),
    DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceDisc_DiscRp DEFAULT(0),

    CONSTRAINT PK_BTR_InvoiceDisc PRIMARY KEY CLUSTERED (InvoiceDiscId)
)
GO

CREATE INDEX IX_BTR_InvoiceDisc_InvoiceId
    ON BTR_InvoiceDisc (InvoiceId, InvoiceDiscId)
GO

-- ============================================================
-- File: \PurchaseContext\BTR_InvoiceItem.sql
-- ============================================================

CREATE TABLE BTR_InvoiceItem(
    InvoiceId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_InvoiceId DEFAULT(''),
    InvoiceItemId VARCHAR(17) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_InvoiceItemId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_InvoiceItem_NoUrut DEFAULT(0),
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_BrgId DEFAULT(''),

    HrgInputStr VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_HrgInputStr DEFAULT(''),
    HrgDetilStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_HrgDetilStr DEFAULT(''),

    QtyInputStr VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_QtyInputStr DEFAULT(''),
    QtyDetilStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_QtyDetilStr DEFAULT(''),
    
    QtyBesar INT NOT NULL CONSTRAINT DF_BTR_InvoiceItem_QtyBesar DEFAULT(0),
    SatBesar VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_SatBesar DEFAULT(''),
    Conversion INT NOT NULL CONSTRAINT DF_BTR_InvoiceItem_Conversion DEFAULT(0),
    HppSatBesar DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_HppSatBesar DEFAULT(0),
    
    QtyKecil INT NOT NULL CONSTRAINT DF_BTR_InvoiceItem_QtyKecil DEFAULT(0),
    SatKecil VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_SatKecil DEFAULT(''),
    HppSatKecil DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_HppSatKecil DEFAULT(0),
    
    QtyBeli INT NOT NULL CONSTRAINT DF_BTR_InvoiceItem_QtyJual DEFAULT(0),
    HppSat DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_HppSat DEFAULT(0),
    SubTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_SubTotal DEFAULT(0),
    
    QtyBonus INT NOT NULL CONSTRAINT DF_BTR_InvoiceItem_QtyBonus DEFAULT(0),
    QtyPotStok INT NOT NULL CONSTRAINT DF_BTR_InvoiceItem_QtyPotStok DEFAULT(0),
    
    DiscInputStr VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_DiscInputStr DEFAULT(''),
    DiscDetilStr VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_DiscDetilStr DEFAULT(''),
    DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_DiscRp DEFAULT(0),

    DppProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_DppProsen DEFAULT(0),
    DppRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_DppRp DEFAULT(0),
    PpnProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_PpnProsen DEFAULT(0),
    PpnRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_PpnRp DEFAULT(0),
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_InvoiceItem_Total DEFAULT(0),
    
    CONSTRAINT PK_BTR_InvoiceItem PRIMARY KEY CLUSTERED (InvoiceItemId)
)

-- ============================================================
-- File: \PurchaseContext\BTR_ReturBeli.sql
-- ============================================================

CREATE TABLE BTR_ReturBeli
(
    ReturBeliId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_ReturBeli_ReturBeliId DEFAULT(''),    
    ReturBeliDate DATETIME NOT NULL CONSTRAINT DF_BTR_ReturBeli_ReturBeliDate DEFAULT('3000-01-01'), 
    ReturBeliCode VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_ReturBeli_ReturBeliCode DEFAULT(''), 

    SupplierId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_ReturBeli_SupplierId DEFAULT(''), 
    WarehouseId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_ReturBeli_WarehouseId DEFAULT(''), 

    NoFakturPajak VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_ReturBeli_NoFakturPajak DEFAULT(''), 
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeli_Total DEFAULT(0), 
    Disc DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeli_Discount DEFAULT(0),
    Dpp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeli_Dpp DEFAULT(0),
    Tax DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeli_Tax DEFAULT(0), 
    GrandTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeli_GrandTotal DEFAULT(''),
    Note VARCHAR(128) NOT NULL CONSTRAINT DF_BTR_ReturBeli_Note DEFAULT(''),

    CreateTime DATETIME NOT NULL CONSTRAINT DF_BTR_ReturBeli_CreateTime DEFAULT('3000-01-01'),
    LastUpdate DATETIME NOT NULL CONSTRAINT DF_BTR_ReturBeli_LastUpdate DEFAULT('3000-01-01'),
    UserId VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_ReturBeli_UserId DEFAULT(''),

    VoidDate DATETIME NOT NULL CONSTRAINT DF_BTR_ReturBeli_VoidDate DEFAULT('3000-01-01'),
    UserIdVoid VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_ReturBeli_UserIdVoid DEFAULT(''),

    
    CONSTRAINT PK_BTR_ReturBeli PRIMARY KEY CLUSTERED (ReturBeliId)
)

-- ============================================================
-- File: \PurchaseContext\BTR_ReturBeliDisc.sql
-- ============================================================

CREATE TABLE BTR_ReturBeliDisc
(
    ReturBeliId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_ReturBeliDisc_DiscId DEFAULT(''),
    ReturBeliItemId VARCHAR(17) NOT NULL CONSTRAINT DF_BTR_ReturBeliDisc_DiscItemId DEFAULT(''),
    ReturBeliDiscId VARCHAR(19) NOT NULL CONSTRAINT DF_BTR_ReturBeliDisc_DiscDiscId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_ReturBeliDisc_NoUrut DEFAULT(0),
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_ReturBeliDisc_BrgId DEFAULT(''),
    DiscProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliDisc_DiscProsen DEFAULT(0),
    DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliDisc_DiscRp DEFAULT(0),

    CONSTRAINT PK_BTR_ReturBeliDisc PRIMARY KEY CLUSTERED (ReturBeliDiscId)
)
GO

CREATE INDEX IX_BTR_ReturBeliDisc_ReturBeliId
    ON BTR_ReturBeliDisc (ReturBeliId, ReturBeliDiscId)
GO

-- ============================================================
-- File: \PurchaseContext\BTR_ReturBeliItem.sql
-- ============================================================

CREATE TABLE BTR_ReturBeliItem(
    ReturBeliId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_ReturBeliId DEFAULT(''),
    ReturBeliItemId VARCHAR(17) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_ReturBeliItemId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_NoUrut DEFAULT(0),
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_BrgId DEFAULT(''),

    HrgInputStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_HrgInputStr DEFAULT(''),
    HrgDetilStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_HrgDetilStr DEFAULT(''),

    QtyInputStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_QtyInputStr DEFAULT(''),
    QtyDetilStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_QtyDetilStr DEFAULT(''),
    
    QtyBesar INT NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_QtyBesar DEFAULT(0),
    SatBesar VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_SatBesar DEFAULT(''),
    Conversion INT NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_Conversion DEFAULT(0),
    HppSatBesar DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_HppSatBesar DEFAULT(0),
    
    QtyKecil INT NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_QtyKecil DEFAULT(0),
    SatKecil VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_SatKecil DEFAULT(''),
    HppSatKecil DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_HppSatKecil DEFAULT(0),
    
    QtyBeli INT NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_QtyJual DEFAULT(0),
    HppSat DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_HppSat DEFAULT(0),
    SubTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_SubTotal DEFAULT(0),
    
    DiscInputStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_DiscInputStr DEFAULT(''),
    DiscDetilStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_DiscDetilStr DEFAULT(''),
    DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_DiscRp DEFAULT(0),

    DppProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_DppProsen DEFAULT(0),
    DppRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_DppRp DEFAULT(0),
    PpnProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_PpnProsen DEFAULT(0),
    PpnRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_PpnRp DEFAULT(0),
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_Total DEFAULT(0),

    StokStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_ReturBeliItem_StokStr DEFAULT(''),
    
    CONSTRAINT PK_BTR_ReturBeliItem PRIMARY KEY CLUSTERED (ReturBeliItemId)
)

-- ============================================================
-- File: \PurchaseContext\BTR_Supplier.sql
-- ============================================================

CREATE TABLE BTR_Supplier(
    SupplierId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Supplier_SupplierId DEFAULT(''),
    SupplierName VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Supplier_SupplierName DEFAULT(''),
    SupplierCode VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_Supplier_SupplierCode DEFAULT(''),

    Address1 VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Supplier_Address1 DEFAULT(''),
    Address2 VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Supplier_Address2 DEFAULT(''),
    Kota VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Supplier_Kota DEFAULT(''),
    KodePos VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_Supplier_KodePos DEFAULT(''),
    NoTelp VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Supplier_NoTelp DEFAULT(''),
    NoFax VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Supplier_NoFax DEFAULT(''),
    ContactPerson VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Supplier_ContactPerson DEFAULT(''),
    Npwp VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Supplier_Npwp DEFAULT(''),
    NoPkp VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Supplier_NoPkp DEFAULT(''),
    Keyword VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Supplier_Keyword DEFAULT(''),
    DepoId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_Supplier_DepoId DEFAULT(''),
    
    CONSTRAINT PK_BTR_Supplier PRIMARY KEY CLUSTERED (SupplierId)
)

-- ============================================================
-- File: \ReportingContext\BTRPD_CashFlowCollectionRisk.sql
-- ============================================================

CREATE TABLE BTRPD_CashFlowCollectionRisk
(
    CashFlowCollectionRiskId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_CashFlowCollectionRiskId DEFAULT(''),
    SnapshotKey              VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_SortOrder DEFAULT(0),
    RiskKey                  VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_RiskKey DEFAULT(''),
    RiskLabel                VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_RiskLabel DEFAULT(''),
    EntityType               VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_EntityType DEFAULT(''),
    EntityId                 VARCHAR(13)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_EntityId DEFAULT(''),
    EntityName               VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_EntityName DEFAULT(''),
    Amount                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_Amount DEFAULT(0),
    DueOrAgingText           VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_DueOrAgingText DEFAULT(''),
    RuleExplanation          VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_RuleExplanation DEFAULT(''),
    ReportRoute              VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowCollectionRisk_ReportRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CashFlowCollectionRisk PRIMARY KEY CLUSTERED (CashFlowCollectionRiskId)
)
GO

CREATE INDEX IX_BTRPD_CashFlowCollectionRisk_SnapshotKey_SortOrder
    ON BTRPD_CashFlowCollectionRisk (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CashFlowDailyPace.sql
-- ============================================================

CREATE TABLE BTRPD_CashFlowDailyPace
(
    CashFlowDailyPaceId      VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_CashFlowDailyPaceId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_SnapshotKey DEFAULT('CURRENT'),
    PaceDate                 DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_PaceDate DEFAULT('3000-01-01'),
    DayOfMonth               INT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_DayOfMonth DEFAULT(0),
    IsElapsed                BIT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_IsElapsed DEFAULT(0),
    ActualCashAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_ActualCashAmount DEFAULT(0),
    ActualCollectionAmount   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_ActualCollectionAmount DEFAULT(0),
    ProjectedDailyCashAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_ProjectedDailyCashAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_CashFlowDailyPace PRIMARY KEY CLUSTERED (CashFlowDailyPaceId)
)
GO

CREATE INDEX IX_BTRPD_CashFlowDailyPace_SnapshotKey_PaceDate
    ON BTRPD_CashFlowDailyPace (SnapshotKey, PaceDate)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CashFlowForecastKpi.sql
-- ============================================================

CREATE TABLE BTRPD_CashFlowForecastKpi
(
    SnapshotKey                         VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                         DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                          INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_PeriodYear DEFAULT(0),
    PeriodMonth                         INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_PeriodMonth DEFAULT(0),
    BusinessDate                        DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    DaysInMonth                         INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_DaysInMonth DEFAULT(0),
    DaysElapsed                         INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_DaysElapsed DEFAULT(0),
    DaysRemaining                       INT            NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_DaysRemaining DEFAULT(0),
    CashCollectedMtd                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_CashCollectedMtd DEFAULT(0),
    MonthCollections                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_MonthCollections DEFAULT(0),
    MonthFakturOmzet                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_MonthFakturOmzet DEFAULT(0),
    DailyCashCollectionAverage          DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_DailyCashCollectionAverage DEFAULT(0),
    DailyCollectionAverage              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_DailyCollectionAverage DEFAULT(0),
    ExpectedCashCollection              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_ExpectedCashCollection DEFAULT(0),
    ProjectedMonthEndTotalCollections   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_ProjectedMonthEndTotalCollections DEFAULT(0),
    CollectionForecastPercent           DECIMAL(9,4)   NULL,
    RecoveryVsBillingPercent            DECIMAL(9,4)   NULL,
    RecoveryVsBillingForecastPercent    DECIMAL(9,4)   NULL,
    RemainingCollectionTarget           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_RemainingCollectionTarget DEFAULT(0),
    RequiredDailyCollection             DECIMAL(18,2)  NULL,
    OutstandingDueRemaining             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_OutstandingDueRemaining DEFAULT(0),
    OverdueOutstanding                  DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_OverdueOutstanding DEFAULT(0),
    CollectionGap                       DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_CollectionGap DEFAULT(0),
    ForecastVarianceCash                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_ForecastVarianceCash DEFAULT(0),
    ExpectedCollectionRatePercent       DECIMAL(9,4)   NULL,
    BestCaseCash                        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_BestCaseCash DEFAULT(0),
    WorstCaseCash                       DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_WorstCaseCash DEFAULT(0),
    ForecastConfidence                  VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_ForecastConfidence DEFAULT(''),
    ForecastRiskBand                    VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_ForecastRiskBand DEFAULT(''),
    LastRefreshLogId                    VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CashFlowForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CashFlowForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CashFlowRecoveryTrend.sql
-- ============================================================

CREATE TABLE BTRPD_CashFlowRecoveryTrend
(
    CashFlowRecoveryTrendId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_CashFlowRecoveryTrendId DEFAULT(''),
    SnapshotKey             VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_SnapshotKey DEFAULT('CURRENT'),
    TrendDate               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_TrendDate DEFAULT('3000-01-01'),
    DayOfMonth              INT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_DayOfMonth DEFAULT(0),
    IsElapsed               BIT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_IsElapsed DEFAULT(0),
    CumulativeCollections   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_CumulativeCollections DEFAULT(0),
    CumulativeBilling       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_CumulativeBilling DEFAULT(0),

    CONSTRAINT PK_BTRPD_CashFlowRecoveryTrend PRIMARY KEY CLUSTERED (CashFlowRecoveryTrendId)
)
GO

CREATE INDEX IX_BTRPD_CashFlowRecoveryTrend_SnapshotKey_TrendDate
    ON BTRPD_CashFlowRecoveryTrend (SnapshotKey, TrendDate)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionAging.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionAging
(
    CollectionAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_CollectionAgingId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_SnapshotKey DEFAULT('CURRENT'),
    BucketKey           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_BucketKey DEFAULT(''),
    BucketLabel         VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_BucketLabel DEFAULT(''),
    Amount              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_Amount DEFAULT(0),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionAging_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionAging PRIMARY KEY CLUSTERED (CollectionAgingId),
    CONSTRAINT UX_BTRPD_CollectionAging_SnapshotKey_BucketKey UNIQUE (SnapshotKey, BucketKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionAttention.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionAttention
(
    CollectionAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_CollectionAttentionId DEFAULT(''),
    SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SnapshotKey DEFAULT('CURRENT'),
    EntityType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityType DEFAULT(''),
    EntityId              VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityId DEFAULT(''),
    EntityCode            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityCode DEFAULT(''),
    EntityName            VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityName DEFAULT(''),
    SignalKey             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SignalKey DEFAULT(''),
    SignalLabel           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SignalLabel DEFAULT(''),
    ValueAmount           DECIMAL(18,2) NULL,
    ValueText             VARCHAR(100)  NULL,
    WilayahName           VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_WilayahName DEFAULT(''),
    ReportRoute           VARCHAR(100)  NULL,
    SortOrder             INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionAttention PRIMARY KEY CLUSTERED (CollectionAttentionId)
)
GO

CREATE INDEX IX_BTRPD_CollectionAttention_SnapshotKey_SortOrder
    ON BTRPD_CollectionAttention (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionKpi.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionKpi
(
    SnapshotKey                   VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                   DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                    INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PeriodYear DEFAULT(0),
    PeriodMonth                   INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PeriodMonth DEFAULT(0),
    OverdueExposure               DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_OverdueExposure DEFAULT(0),
    AgingOver90Exposure           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_AgingOver90Exposure DEFAULT(0),
    OverdueConcentrationPercent   DECIMAL(9,4)  NULL,
    CashCollectedMtd              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_CashCollectedMtd DEFAULT(0),
    MonthCollections              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_MonthCollections DEFAULT(0),
    MonthFakturOmzet              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_MonthFakturOmzet DEFAULT(0),
    RecoveryVsBillingPercent      DECIMAL(9,4)  NULL,
    PaymentMixCashAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PaymentMixCashAmount DEFAULT(0),
    PaymentMixGiroAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PaymentMixGiroAmount DEFAULT(0),
    PaymentMixAdjustmentAmount    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PaymentMixAdjustmentAmount DEFAULT(0),
    PaymentMixCashPercent         DECIMAL(9,4)  NULL,
    PaymentMixGiroPercent         DECIMAL(9,4)  NULL,
    PaymentMixAdjustmentPercent   DECIMAL(9,4)  NULL,
    LegacyDebtCount               INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_LegacyDebtCount DEFAULT(0),
    ChronicOverdueCount           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_ChronicOverdueCount DEFAULT(0),
    WilayahHotspotCount           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_WilayahHotspotCount DEFAULT(0),
    LowRecoveryVsBillingCount     INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_LowRecoveryVsBillingCount DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionOptimizationActionDist.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionOptimizationActionDist
(
    CollectionOptimizationActionDistId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_Id DEFAULT(''),
    SnapshotKey                        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_SnapshotKey DEFAULT('CURRENT'),
    ActionCategoryKey                  VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel                VARCHAR(60)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_ActionCategoryLabel DEFAULT(''),
    CustomerCount                      INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_CustomerCount DEFAULT(0),
    ImpactTotal                        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_ImpactTotal DEFAULT(0),
    SortOrder                          INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionOptimizationActionDist PRIMARY KEY CLUSTERED (CollectionOptimizationActionDistId)
)
GO

CREATE INDEX IX_BTRPD_CollectionOptimizationActionDist_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationActionDist (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionOptimizationImpact.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionOptimizationImpact
(
    CollectionOptimizationImpactId   VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_SortOrder DEFAULT(0),
    CustomerCode                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_CustomerName DEFAULT(''),
    WilayahName                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_WilayahName DEFAULT(''),
    SalesPersonName                  VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_SalesPersonName DEFAULT(''),
    ActionCategoryKey                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel              VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_ActionCategoryLabel DEFAULT(''),
    CollectionImpactAmount           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_CollectionImpactAmount DEFAULT(0),
    OverdueBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_OverdueBalance DEFAULT(0),
    DueWithin7Days                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_DueWithin7Days DEFAULT(0),
    ReportRoute                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationImpact_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationImpact PRIMARY KEY CLUSTERED (CollectionOptimizationImpactId)
)
GO

CREATE INDEX IX_BTRPD_CollectionOptimizationImpact_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationImpact (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionOptimizationKpi.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionOptimizationKpi
(
    SnapshotKey                     VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                       DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_GeneratedAt DEFAULT('3000-01-01'),
    BusinessDate                      DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_BusinessDate DEFAULT('3000-01-01'),
    ActionsTodayCount                 INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_ActionsTodayCount DEFAULT(0),
    ImmediateCollectionCount          INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_ImmediateCollectionCount DEFAULT(0),
    ProactiveReminderCount            INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_ProactiveReminderCount DEFAULT(0),
    CreditReviewCount                 INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_CreditReviewCount DEFAULT(0),
    SalesRecoveryCount                INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_SalesRecoveryCount DEFAULT(0),
    EscalateManagementCount           INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_EscalateManagementCount DEFAULT(0),
    CollectionImpactTotal             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_CollectionImpactTotal DEFAULT(0),
    ImmediateImpactTotal              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_ImmediateImpactTotal DEFAULT(0),
    OverdueExposure                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_OverdueExposure DEFAULT(0),
    DueWithin7Days                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_DueWithin7Days DEFAULT(0),
    RecoveryVsBillingPercent          DECIMAL(9,4)   NULL,
    DeferNoActionCount                INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_DeferNoActionCount DEFAULT(0),
    PlanningConfidence                VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_PlanningConfidence DEFAULT(''),
    ExecutiveSummaryText              VARCHAR(2000)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_ExecutiveSummaryText DEFAULT(''),
    LastRefreshLogId                  VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionOptimizationPriority.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionOptimizationPriority
(
    CollectionOptimizationPriorityId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SortOrder DEFAULT(0),
    CollectionPriorityScore          INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CollectionPriorityScore DEFAULT(0),
    CustomerCode                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CustomerName DEFAULT(''),
    WilayahName                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_WilayahName DEFAULT(''),
    SalesPersonName                  VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SalesPersonName DEFAULT(''),
    Klasifikasi                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_Klasifikasi DEFAULT(''),
    ActionCategoryKey                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel              VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionCategoryLabel DEFAULT(''),
    RecommendedActionKey             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_RecommendedActionKey DEFAULT(''),
    RecommendedActionLabel           VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_RecommendedActionLabel DEFAULT(''),
    ActionOwner                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionOwner DEFAULT(''),
    OpenBalance                      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_OpenBalance DEFAULT(0),
    OverdueBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_OverdueBalance DEFAULT(0),
    DueWithin7Days                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_DueWithin7Days DEFAULT(0),
    CollectionImpactAmount           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CollectionImpactAmount DEFAULT(0),
    M29Category                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_M29Category DEFAULT(''),
    M29RecommendationKey             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_M29RecommendationKey DEFAULT(''),
    M29PrimarySignalKey              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_M29PrimarySignalKey DEFAULT(''),
    MinDaysUntilDue                  INT            NULL,
    CreditUtilizationPercent         DECIMAL(9,4)   NULL,
    SelectionReasonText              VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SelectionReasonText DEFAULT(''),
    PriorityReasonText               VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_PriorityReasonText DEFAULT(''),
    ActionReasonText                 VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionReasonText DEFAULT(''),
    TriggeredRuleIds                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_TriggeredRuleIds DEFAULT(''),
    ReportRoute                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationPriority PRIMARY KEY CLUSTERED (CollectionOptimizationPriorityId)
)
GO

CREATE INDEX IX_BTRPD_CollectionOptimizationPriority_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationPriority (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionOptimizationQueue.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionOptimizationQueue
(
    CollectionOptimizationQueueId    VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_SnapshotKey DEFAULT('CURRENT'),
    QueueKey                         VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_QueueKey DEFAULT(''),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_SortOrder DEFAULT(0),
    CollectionPriorityScore          INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CollectionPriorityScore DEFAULT(0),
    CustomerCode                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CustomerName DEFAULT(''),
    WilayahName                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_WilayahName DEFAULT(''),
    SalesPersonName                  VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_SalesPersonName DEFAULT(''),
    ActionCategoryKey                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel              VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ActionCategoryLabel DEFAULT(''),
    RecommendedActionKey             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_RecommendedActionKey DEFAULT(''),
    RecommendedActionLabel           VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_RecommendedActionLabel DEFAULT(''),
    ActionOwner                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ActionOwner DEFAULT(''),
    OverdueBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_OverdueBalance DEFAULT(0),
    DueWithin7Days                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_DueWithin7Days DEFAULT(0),
    CollectionImpactAmount           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CollectionImpactAmount DEFAULT(0),
    M29Category                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_M29Category DEFAULT(''),
    QueueReasonText                  VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_QueueReasonText DEFAULT(''),
    ReportRoute                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationQueue PRIMARY KEY CLUSTERED (CollectionOptimizationQueueId)
)
GO

CREATE INDEX IX_BTRPD_CollectionOptimizationQueue_SnapshotKey_QueueKey_SortOrder
    ON BTRPD_CollectionOptimizationQueue (SnapshotKey, QueueKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionOptimizationWorkload.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionOptimizationWorkload
(
    CollectionOptimizationWorkloadId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_SnapshotKey DEFAULT('CURRENT'),
    WorkloadType                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_WorkloadType DEFAULT(''),
    EntityKey                        VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_EntityKey DEFAULT(''),
    EntityLabel                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_EntityLabel DEFAULT(''),
    ActionCount                      INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_ActionCount DEFAULT(0),
    ImmediateCount                   INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_ImmediateCount DEFAULT(0),
    ImpactTotal                      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_ImpactTotal DEFAULT(0),
    OverdueExposure                  DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_OverdueExposure DEFAULT(0),
    IsHotspot                        BIT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_IsHotspot DEFAULT(0),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionOptimizationWorkload PRIMARY KEY CLUSTERED (CollectionOptimizationWorkloadId)
)
GO

CREATE INDEX IX_BTRPD_CollectionOptimizationWorkload_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationWorkload (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionTopOverdueCustomer.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionTopOverdueCustomer
(
    CollectionTopOverdueCustomerId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_CollectionTopOverdueCustomerId DEFAULT(''),
    SnapshotKey                      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_SnapshotKey DEFAULT('CURRENT'),
    Rank                             INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_Rank DEFAULT(0),
    CustomerCode                     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_CustomerName DEFAULT(''),
    OverdueBalance                   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_OverdueBalance DEFAULT(0),
    PercentOfTotal                   DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CollectionTopOverdueCustomer PRIMARY KEY CLUSTERED (CollectionTopOverdueCustomerId),
    CONSTRAINT UX_BTRPD_CollectionTopOverdueCustomer_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionTopOverdueSalesman.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionTopOverdueSalesman
(
    CollectionTopOverdueSalesmanId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_CollectionTopOverdueSalesmanId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SnapshotKey DEFAULT('CURRENT'),
    Rank                           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_Rank DEFAULT(0),
    SalesPersonId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonId DEFAULT(''),
    SalesPersonCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonCode DEFAULT(''),
    SalesPersonName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonName DEFAULT(''),
    OverdueBalance                 DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_OverdueBalance DEFAULT(0),
    PercentOfTotal                 DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CollectionTopOverdueSalesman PRIMARY KEY CLUSTERED (CollectionTopOverdueSalesmanId),
    CONSTRAINT UX_BTRPD_CollectionTopOverdueSalesman_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CollectionTopOverdueWilayah.sql
-- ============================================================

CREATE TABLE BTRPD_CollectionTopOverdueWilayah
(
    CollectionTopOverdueWilayahId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_CollectionTopOverdueWilayahId DEFAULT(''),
    SnapshotKey                   VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_SnapshotKey DEFAULT('CURRENT'),
    Rank                          INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_Rank DEFAULT(0),
    WilayahId                     VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_WilayahId DEFAULT(''),
    WilayahName                   VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_WilayahName DEFAULT(''),
    OverdueBalance                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_OverdueBalance DEFAULT(0),
    PercentOfTotal                DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CollectionTopOverdueWilayah PRIMARY KEY CLUSTERED (CollectionTopOverdueWilayahId),
    CONSTRAINT UX_BTRPD_CollectionTopOverdueWilayah_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerAttention.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerAttention
(
    CustomerAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_CustomerAttentionId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_SnapshotKey DEFAULT('CURRENT'),
    CustomerId          VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_CustomerId DEFAULT(''),
    CustomerCode        VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_CustomerCode DEFAULT(''),
    CustomerName        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_CustomerName DEFAULT(''),
    SignalKey           VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_SignalKey DEFAULT(''),
    SignalLabel         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_SignalLabel DEFAULT(''),
    ValueAmount         DECIMAL(18,2) NULL,
    ValueText           VARCHAR(50)   NULL,
    WilayahName         VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_WilayahName DEFAULT(''),
    LastInvoicingSalesmanName VARCHAR(50) NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_LastInvoicingSalesmanName DEFAULT(''),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerAttention PRIMARY KEY CLUSTERED (CustomerAttentionId)
)
GO

CREATE INDEX IX_BTRPD_CustomerAttention_SnapshotKey_SortOrder
    ON BTRPD_CustomerAttention (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerKpi.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerKpi
(
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_PeriodYear DEFAULT(0),
    PeriodMonth               INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_PeriodMonth DEFAULT(0),
    TotalOmzet                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_TotalOmzet DEFAULT(0),
    TotalPiutang              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_TotalPiutang DEFAULT(0),
    ActiveCustomerCount       INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_ActiveCustomerCount DEFAULT(0),
    DormantCustomerCount      INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_DormantCustomerCount DEFAULT(0),
    OverdueCustomerCount      INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_OverdueCustomerCount DEFAULT(0),
    PlafondBreachCount        INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_PlafondBreachCount DEFAULT(0),
    SuspendedWithSalesCount   INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_SuspendedWithSalesCount DEFAULT(0),
    AgingOver90Amount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_AgingOver90Amount DEFAULT(0),
    TopOmzetCustomerPercent   DECIMAL(9,4)  NULL,
    TopPiutangCustomerPercent DECIMAL(9,4)  NULL,
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerPortfolioActionDist.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerPortfolioActionDist

(

    CustomerPortfolioActionDistId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_Id DEFAULT(''),

    SnapshotKey                   VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_SnapshotKey DEFAULT('CURRENT'),

    PrimaryActionKey              VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_PrimaryActionKey DEFAULT(''),

    PrimaryActionLabel            VARCHAR(60)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_PrimaryActionLabel DEFAULT(''),

    CustomerCount                 INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_CustomerCount DEFAULT(0),

    SortOrder                     INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_SortOrder DEFAULT(0),



    CONSTRAINT PK_BTRPD_CustomerPortfolioActionDist PRIMARY KEY CLUSTERED (CustomerPortfolioActionDistId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioActionDist_SnapshotKey

    ON BTRPD_CustomerPortfolioActionDist (SnapshotKey, SortOrder)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerPortfolioConcentration.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerPortfolioConcentration

(

    CustomerPortfolioConcentrationId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_Id DEFAULT(''),

    SnapshotKey                      VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_SnapshotKey DEFAULT('CURRENT'),

    ConcentrationType                VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_ConcentrationType DEFAULT(''),

    SortOrder                        INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_SortOrder DEFAULT(0),

    Rank                             INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_Rank DEFAULT(0),

    CustomerCode                     VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_CustomerCode DEFAULT(''),

    CustomerName                     VARCHAR(100) NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_CustomerName DEFAULT(''),

    Amount                           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_Amount DEFAULT(0),

    PercentOfTotal                   DECIMAL(9,4) NULL,



    CONSTRAINT PK_BTRPD_CustomerPortfolioConcentration PRIMARY KEY CLUSTERED (CustomerPortfolioConcentrationId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioConcentration_SnapshotKey_Type

    ON BTRPD_CustomerPortfolioConcentration (SnapshotKey, ConcentrationType, SortOrder)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerPortfolioCustomer.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerPortfolioCustomer

(

    CustomerPortfolioCustomerId   VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_Id DEFAULT(''),

    SnapshotKey                   VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_SnapshotKey DEFAULT('CURRENT'),

    SortOrder                     INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_SortOrder DEFAULT(0),

    CustomerKey                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerKey DEFAULT(''),

    CustomerId                    VARCHAR(13)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerId DEFAULT(''),

    CustomerCode                  VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerCode DEFAULT(''),

    CustomerName                  VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerName DEFAULT(''),

    WilayahName                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_WilayahName DEFAULT(''),

    Klasifikasi                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_Klasifikasi DEFAULT(''),

    LifecycleStage                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_LifecycleStage DEFAULT(''),

    LifecycleLabel                VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_LifecycleLabel DEFAULT(''),

    PortfolioTier                 VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_PortfolioTier DEFAULT(''),

    TierLabel                     VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_TierLabel DEFAULT(''),

    PrimaryActionKey              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_PrimaryActionKey DEFAULT(''),

    PrimaryActionLabel            VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_PrimaryActionLabel DEFAULT(''),

    ActionOwner                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_ActionOwner DEFAULT(''),

    ActionReasonText              VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_ActionReasonText DEFAULT(''),

    TriggeredRuleIds              VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_TriggeredRuleIds DEFAULT(''),

    MtdOmzet                      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_MtdOmzet DEFAULT(0),

    OpenBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_OpenBalance DEFAULT(0),

    OverdueBalance                DECIMAL(18,2)  NULL,

    FakturCount6Mo                INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_FakturCount6Mo DEFAULT(0),

    IsActiveMtd                   BIT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_IsActiveMtd DEFAULT(0),

    LastPurchaseDate              DATETIME       NULL,

    FirstPurchaseDate             DATETIME       NULL,

    M29Category                   VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_M29Category DEFAULT(''),

    M29PrimarySignalKey           VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_M29PrimarySignalKey DEFAULT(''),

    SalesPersonName               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_SalesPersonName DEFAULT(''),

    SalesmanAchievementPercent    DECIMAL(9,4)   NULL,

    SalesmanHighPiutangExposure   BIT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_SalesmanHighPiutangExposure DEFAULT(0),

    IsAttention                   BIT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_IsAttention DEFAULT(0),

    PortfolioPriorityScore        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_PortfolioPriorityScore DEFAULT(0),

    M30LinkRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_M30LinkRoute DEFAULT(''),

    CustomerReportRoute           VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerReportRoute DEFAULT(''),

    DrillDownRouteM17             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_DrillDownRouteM17 DEFAULT(''),

    DrillDownRouteM29             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_DrillDownRouteM29 DEFAULT(''),

    ValueDisclaimer               VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_ValueDisclaimer DEFAULT(''),



    CONSTRAINT PK_BTRPD_CustomerPortfolioCustomer PRIMARY KEY CLUSTERED (CustomerPortfolioCustomerId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioCustomer_SnapshotKey_IsAttention

    ON BTRPD_CustomerPortfolioCustomer (SnapshotKey, IsAttention)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioCustomer_SnapshotKey_CustomerCode

    ON BTRPD_CustomerPortfolioCustomer (SnapshotKey, CustomerCode)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerPortfolioKpi.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerPortfolioKpi

(

    SnapshotKey                 VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_SnapshotKey DEFAULT('CURRENT'),

    GeneratedAt                   DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_GeneratedAt DEFAULT('3000-01-01'),

    BusinessDate                  DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_BusinessDate DEFAULT('3000-01-01'),

    PortfolioHealthScore          DECIMAL(9,4)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_PortfolioHealthScore DEFAULT(0),

    PortfolioHealthyPercent       DECIMAL(9,4)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_PortfolioHealthyPercent DEFAULT(0),

    TotalCustomerCount            INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_TotalCustomerCount DEFAULT(0),

    AttentionCustomerCount        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_AttentionCustomerCount DEFAULT(0),

    StrategicCustomerCount        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_StrategicCustomerCount DEFAULT(0),

    StrategicAtRiskCount          INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_StrategicAtRiskCount DEFAULT(0),

    CustomersAtRiskCount          INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_CustomersAtRiskCount DEFAULT(0),

    WorkingCapitalTiedAmount      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_WorkingCapitalTiedAmount DEFAULT(0),

    TotalMtdOmzet                 DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_TotalMtdOmzet DEFAULT(0),

    TotalOpenBalance              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_TotalOpenBalance DEFAULT(0),

    NeverPurchasedCount           INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_NeverPurchasedCount DEFAULT(0),

    DormantCount                  INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_DormantCount DEFAULT(0),

    DecliningCount                INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_DecliningCount DEFAULT(0),

    ExecutiveSummaryText          VARCHAR(2000)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_ExecutiveSummaryText DEFAULT(''),

    ValueDisclaimerText           VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_ValueDisclaimerText DEFAULT(''),

    LastRefreshLogId              VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_LastRefreshLogId DEFAULT(''),



    CONSTRAINT PK_BTRPD_CustomerPortfolioKpi PRIMARY KEY CLUSTERED (SnapshotKey)

)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerPortfolioLifecycleDist.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerPortfolioLifecycleDist

(

    CustomerPortfolioLifecycleDistId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_Id DEFAULT(''),

    SnapshotKey                      VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_SnapshotKey DEFAULT('CURRENT'),

    LifecycleStage                   VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_LifecycleStage DEFAULT(''),

    LifecycleLabel                   VARCHAR(60)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_LifecycleLabel DEFAULT(''),

    CustomerCount                    INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_CustomerCount DEFAULT(0),

    SortOrder                        INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_SortOrder DEFAULT(0),



    CONSTRAINT PK_BTRPD_CustomerPortfolioLifecycleDist PRIMARY KEY CLUSTERED (CustomerPortfolioLifecycleDistId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioLifecycleDist_SnapshotKey

    ON BTRPD_CustomerPortfolioLifecycleDist (SnapshotKey, SortOrder)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerPortfolioPriority.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerPortfolioPriority

(

    CustomerPortfolioPriorityId   VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_Id DEFAULT(''),

    SnapshotKey                   VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_SnapshotKey DEFAULT('CURRENT'),

    SortOrder                     INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_SortOrder DEFAULT(0),

    PortfolioPriorityScore        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_PortfolioPriorityScore DEFAULT(0),

    CustomerKey                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerKey DEFAULT(''),

    CustomerId                    VARCHAR(13)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerId DEFAULT(''),

    CustomerCode                  VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerCode DEFAULT(''),

    CustomerName                  VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerName DEFAULT(''),

    WilayahName                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_WilayahName DEFAULT(''),

    Klasifikasi                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_Klasifikasi DEFAULT(''),

    LifecycleStage                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_LifecycleStage DEFAULT(''),

    LifecycleLabel                VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_LifecycleLabel DEFAULT(''),

    PortfolioTier                 VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_PortfolioTier DEFAULT(''),

    TierLabel                     VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_TierLabel DEFAULT(''),

    PrimaryActionKey              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_PrimaryActionKey DEFAULT(''),

    PrimaryActionLabel            VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_PrimaryActionLabel DEFAULT(''),

    ActionOwner                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_ActionOwner DEFAULT(''),

    ActionReasonText              VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_ActionReasonText DEFAULT(''),

    TriggeredRuleIds              VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_TriggeredRuleIds DEFAULT(''),

    MtdOmzet                      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_MtdOmzet DEFAULT(0),

    OpenBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_OpenBalance DEFAULT(0),

    OverdueBalance                DECIMAL(18,2)  NULL,

    M29Category                   VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_M29Category DEFAULT(''),

    SalesPersonName               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_SalesPersonName DEFAULT(''),

    SalesmanAchievementPercent    DECIMAL(9,4)   NULL,

    SalesmanHighPiutangExposure   BIT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_SalesmanHighPiutangExposure DEFAULT(0),

    IsAttention                   BIT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_IsAttention DEFAULT(0),

    M30LinkRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_M30LinkRoute DEFAULT(''),

    CustomerReportRoute           VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerReportRoute DEFAULT(''),

    DrillDownRouteM17             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_DrillDownRouteM17 DEFAULT(''),

    DrillDownRouteM29             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_DrillDownRouteM29 DEFAULT(''),



    CONSTRAINT PK_BTRPD_CustomerPortfolioPriority PRIMARY KEY CLUSTERED (CustomerPortfolioPriorityId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioPriority_SnapshotKey_SortOrder

    ON BTRPD_CustomerPortfolioPriority (SnapshotKey, SortOrder)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerPortfolioTierDist.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerPortfolioTierDist

(

    CustomerPortfolioTierDistId  VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_Id DEFAULT(''),

    SnapshotKey                  VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_SnapshotKey DEFAULT('CURRENT'),

    PortfolioTier                VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_PortfolioTier DEFAULT(''),

    TierLabel                    VARCHAR(60)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_TierLabel DEFAULT(''),

    CustomerCount                INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_CustomerCount DEFAULT(0),

    SortOrder                    INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_SortOrder DEFAULT(0),



    CONSTRAINT PK_BTRPD_CustomerPortfolioTierDist PRIMARY KEY CLUSTERED (CustomerPortfolioTierDistId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioTierDist_SnapshotKey

    ON BTRPD_CustomerPortfolioTierDist (SnapshotKey, SortOrder)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerPortfolioWilayah.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerPortfolioWilayah

(

    CustomerPortfolioWilayahId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_Id DEFAULT(''),

    SnapshotKey                VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_SnapshotKey DEFAULT('CURRENT'),

    SortOrder                  INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_SortOrder DEFAULT(0),

    WilayahName                VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_WilayahName DEFAULT(''),

    CustomerCount              INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_CustomerCount DEFAULT(0),

    AttentionCustomerCount     INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_AttentionCustomerCount DEFAULT(0),



    CONSTRAINT PK_BTRPD_CustomerPortfolioWilayah PRIMARY KEY CLUSTERED (CustomerPortfolioWilayahId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioWilayah_SnapshotKey

    ON BTRPD_CustomerPortfolioWilayah (SnapshotKey, SortOrder)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerPrincipalRelationship.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerPrincipalRelationship
(
    CustomerPrincipalRelationshipId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_CustomerPrincipalRelationshipId DEFAULT(''),
    CustomerId                      VARCHAR(6)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_CustomerId DEFAULT(''),
    CustomerName                    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_CustomerName DEFAULT(''),
    SupplierId                      VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_SupplierId DEFAULT(''),
    SupplierName                    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_SupplierName DEFAULT(''),
    FirstTransactionDate            DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_FirstTransactionDate DEFAULT('3000-01-01'),
    LastTransactionDate             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_LastTransactionDate DEFAULT('3000-01-01'),
    RelationshipStatus              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_RelationshipStatus DEFAULT(''),
    KpiId                           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_KpiId DEFAULT('PRN-SALES-001'),
    SalesOutAmount                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_SalesOutAmount DEFAULT(0),
    LineCount                       INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_LineCount DEFAULT(0),
    AsOfDate                        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_AsOfDate DEFAULT('3000-01-01'),
    GeneratedAt                     DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId                VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerPrincipalRelationship PRIMARY KEY CLUSTERED (CustomerPrincipalRelationshipId),
    CONSTRAINT UX_BTRPD_CustomerPrincipalRelationship_CustomerId_SupplierId UNIQUE (CustomerId, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_CustomerPrincipalRelationship_SupplierId_Status
    ON BTRPD_CustomerPrincipalRelationship (SupplierId, RelationshipStatus)
GO

CREATE INDEX IX_BTRPD_CustomerPrincipalRelationship_CustomerId
    ON BTRPD_CustomerPrincipalRelationship (CustomerId)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerPrincipalRelationshipKpi.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerPrincipalRelationshipKpi
(
    SnapshotKey          VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_SnapshotKey DEFAULT('CURRENT'),
    KpiId                VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_KpiId DEFAULT('PRN-SALES-001'),
    AsOfDate             DATETIME     NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_AsOfDate DEFAULT('3000-01-01'),
    HistoricalLimitation VARCHAR(300) NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_HistoricalLimitation DEFAULT(''),
    GeneratedAt          DATETIME     NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId     VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerPrincipalRelationshipKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerRiskForecastAttention.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerRiskForecastAttention
(
    CustomerRiskForecastAttentionId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_CustomerRiskForecastAttentionId DEFAULT(''),
    SnapshotKey                       VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                         INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SortOrder DEFAULT(0),
    CustomerCode                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_CustomerCode DEFAULT(''),
    CustomerName                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_CustomerName DEFAULT(''),
    SignalKey                         VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SignalKey DEFAULT(''),
    SignalLabel                       VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SignalLabel DEFAULT(''),
    Severity                          VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_Severity DEFAULT(''),
    Amount                            DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_Amount DEFAULT(0),
    HorizonText                       VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_HorizonText DEFAULT(''),
    RuleId                            VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_RuleId DEFAULT(''),
    Explanation                       VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_Explanation DEFAULT(''),
    ReportRoute                       VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_ReportRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastAttention PRIMARY KEY CLUSTERED (CustomerRiskForecastAttentionId)
)
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastAttention_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastAttention (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerRiskForecastCustomer.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerRiskForecastCustomer
(
    CustomerRiskForecastCustomerId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CustomerRiskForecastCustomerId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                      INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_SortOrder DEFAULT(0),
    RiskPriorityScore              INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_RiskPriorityScore DEFAULT(0),
    Category                       VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_Category DEFAULT(''),
    CategoryLabel                  VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CategoryLabel DEFAULT(''),
    CustomerCode                   VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CustomerCode DEFAULT(''),
    CustomerName                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CustomerName DEFAULT(''),
    WilayahName                    VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_WilayahName DEFAULT(''),
    SalesPersonName                VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_SalesPersonName DEFAULT(''),
    OpenBalance                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_OpenBalance DEFAULT(0),
    OverdueBalance                 DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_OverdueBalance DEFAULT(0),
    DueWithinHorizon               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_DueWithinHorizon DEFAULT(0),
    Plafond                        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_Plafond DEFAULT(0),
    ProjectedOpenBalance           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_ProjectedOpenBalance DEFAULT(0),
    MtdOmzet                       DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_MtdOmzet DEFAULT(0),
    PriorMonthOmzet                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_PriorMonthOmzet DEFAULT(0),
    DeclineRatio                   DECIMAL(9,4)   NULL,
    DaysSinceLastFaktur            INT            NULL,
    AvgPaymentLagDays              DECIMAL(9,2)   NULL,
    PrimarySignalKey               VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_PrimarySignalKey DEFAULT(''),
    PrimarySignalLabel             VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_PrimarySignalLabel DEFAULT(''),
    ReasonText                     VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_ReasonText DEFAULT(''),
    RecommendationKey              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_RecommendationKey DEFAULT(''),
    RecommendationLabel            VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_RecommendationLabel DEFAULT(''),
    ReportRoute                    VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_ReportRoute DEFAULT(''),
    DrillDownRoute                 VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastCustomer PRIMARY KEY CLUSTERED (CustomerRiskForecastCustomerId)
)
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastCustomer_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastCustomer (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerRiskForecastDist.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerRiskForecastDist
(
    CustomerRiskForecastDistId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_CustomerRiskForecastDistId DEFAULT(''),
    SnapshotKey                VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_SnapshotKey DEFAULT('CURRENT'),
    Category                   VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_Category DEFAULT(''),
    CategoryLabel              VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_CategoryLabel DEFAULT(''),
    CustomerCount              INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_CustomerCount DEFAULT(0),
    SortOrder                  INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastDist PRIMARY KEY CLUSTERED (CustomerRiskForecastDistId)
)
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastDist_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastDist (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerRiskForecastKpi.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerRiskForecastKpi
(
    SnapshotKey                         VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                         DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    BusinessDate                        DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    HorizonDays                         INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HorizonDays DEFAULT(0),
    CustomersForecastedAtRisk           INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CustomersForecastedAtRisk DEFAULT(0),
    HighRiskCustomerCount               INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HighRiskCustomerCount DEFAULT(0),
    CriticalCustomerCount               INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CriticalCustomerCount DEFAULT(0),
    ElevatedRiskReceivable              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_ElevatedRiskReceivable DEFAULT(0),
    ElevatedRiskReceivablePercent       DECIMAL(9,4)   NULL,
    PortfolioHealthScore                DECIMAL(9,4)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_PortfolioHealthScore DEFAULT(0),
    TotalPiutang                        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_TotalPiutang DEFAULT(0),
    ForecastConfidence                  VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_ForecastConfidence DEFAULT(''),
    PaymentDelaySignalCount             INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_PaymentDelaySignalCount DEFAULT(0),
    CreditLimitSignalCount              INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CreditLimitSignalCount DEFAULT(0),
    InactivitySignalCount               INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_InactivitySignalCount DEFAULT(0),
    PurchaseDeclineSignalCount          INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_PurchaseDeclineSignalCount DEFAULT(0),
    CollectionRiskSignalCount           INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CollectionRiskSignalCount DEFAULT(0),
    HealthyCount                        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HealthyCount DEFAULT(0),
    WatchCount                          INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_WatchCount DEFAULT(0),
    AttentionCount                      INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_AttentionCount DEFAULT(0),
    HighRiskCount                       INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HighRiskCount DEFAULT(0),
    CriticalCount                       INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CriticalCount DEFAULT(0),
    ExecutiveSummaryText                VARCHAR(2000)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_ExecutiveSummaryText DEFAULT(''),
    LastRefreshLogId                    VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerRiskForecastRecommendation.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerRiskForecastRecommendation
(
    CustomerRiskForecastRecommendationId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_CustomerRiskForecastRecommendationId DEFAULT(''),
    SnapshotKey                          VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                            INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_SortOrder DEFAULT(0),
    RecommendationKey                    VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_RecommendationKey DEFAULT(''),
    RecommendationLabel                  VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_RecommendationLabel DEFAULT(''),
    CustomerCode                         VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_CustomerCode DEFAULT(''),
    CustomerName                         VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_CustomerName DEFAULT(''),
    Category                             VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_Category DEFAULT(''),
    ReasonText                           VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_ReasonText DEFAULT(''),
    RuleId                               VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_RuleId DEFAULT(''),
    ReportRoute                          VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_ReportRoute DEFAULT(''),
    DrillDownRoute                       VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastRecommendation PRIMARY KEY CLUSTERED (CustomerRiskForecastRecommendationId)
)
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastRecommendation_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastRecommendation (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerRiskForecastSignalMix.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerRiskForecastSignalMix
(
    CustomerRiskForecastSignalMixId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_CustomerRiskForecastSignalMixId DEFAULT(''),
    SnapshotKey                     VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SnapshotKey DEFAULT('CURRENT'),
    SignalFamilyKey                 VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SignalFamilyKey DEFAULT(''),
    SignalFamilyLabel               VARCHAR(50) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SignalFamilyLabel DEFAULT(''),
    CustomerCount                   INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_CustomerCount DEFAULT(0),
    SortOrder                       INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastSignalMix PRIMARY KEY CLUSTERED (CustomerRiskForecastSignalMixId)
)
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastSignalMix_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastSignalMix (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerRiskForecastWilayah.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerRiskForecastWilayah
(
    CustomerRiskForecastWilayahId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_CustomerRiskForecastWilayahId DEFAULT(''),
    SnapshotKey                   VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_SnapshotKey DEFAULT('CURRENT'),
    WilayahName                   VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_WilayahName DEFAULT(''),
    ElevatedRiskCustomerCount     INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_ElevatedRiskCustomerCount DEFAULT(0),
    SortOrder                     INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastWilayah PRIMARY KEY CLUSTERED (CustomerRiskForecastWilayahId)
)
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastWilayah_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastWilayah (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerSegmentation.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerSegmentation
(
    CustomerSegmentationId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_CustomerSegmentationId DEFAULT(''),
    SnapshotKey            VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SnapshotKey DEFAULT('CURRENT'),
    SegmentType            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SegmentType DEFAULT(''),
    SegmentKey             VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SegmentKey DEFAULT(''),
    SegmentLabel           VARCHAR(50) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SegmentLabel DEFAULT(''),
    CustomerCount          INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_CustomerCount DEFAULT(0),
    ActiveCount            INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_ActiveCount DEFAULT(0),
    DormantCount           INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_DormantCount DEFAULT(0),
    SortOrder              INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerSegmentation PRIMARY KEY CLUSTERED (CustomerSegmentationId),
    CONSTRAINT UX_BTRPD_CustomerSegmentation_SnapshotKey_SegmentType_SegmentKey UNIQUE (SnapshotKey, SegmentType, SegmentKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerTopOmzet.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerTopOmzet
(
    CustomerTopOmzetId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerTopOmzetId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_Rank DEFAULT(0),
    CustomerId         VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerId DEFAULT(''),
    CustomerCode       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerCode DEFAULT(''),
    CustomerName       VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerName DEFAULT(''),
    OmzetAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_OmzetAmount DEFAULT(0),
    PercentOfTotal     DECIMAL(9,4)  NULL,
    LastInvoicingSalesmanName VARCHAR(50) NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_LastInvoicingSalesmanName DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerTopOmzet PRIMARY KEY CLUSTERED (CustomerTopOmzetId),
    CONSTRAINT UX_BTRPD_CustomerTopOmzet_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_CustomerTopPiutang.sql
-- ============================================================

CREATE TABLE BTRPD_CustomerTopPiutang
(
    CustomerTopPiutangId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerTopPiutangId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_Rank DEFAULT(0),
    CustomerId           VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerId DEFAULT(''),
    CustomerCode         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerCode DEFAULT(''),
    CustomerName         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_OutstandingBalance DEFAULT(0),
    PercentOfTotal       DECIMAL(9,4)  NULL,
    LastInvoicingSalesmanName VARCHAR(50) NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_LastInvoicingSalesmanName DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerTopPiutang PRIMARY KEY CLUSTERED (CustomerTopPiutangId),
    CONSTRAINT UX_BTRPD_CustomerTopPiutang_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_EntityAnalytics_Attention.sql
-- ============================================================

CREATE TABLE BTRPD_EntityAnalytics_Attention
(
    EntityAnalyticsAttentionId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_Id DEFAULT(''),
    EntityType                 VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_EntityType DEFAULT(''),
    EntityId                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_EntityId DEFAULT(''),
    EntityCode                 VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_EntityCode DEFAULT(''),
    SignalCode                 VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_SignalCode DEFAULT(''),
    SignalCategory             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_SignalCategory DEFAULT(''),
    SignalTitle                VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_SignalTitle DEFAULT(''),
    FirstSeenYear              INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_FirstSeenYear DEFAULT(0),
    FirstSeenMonth             INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_FirstSeenMonth DEFAULT(0),
    LastSeenYear               INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_LastSeenYear DEFAULT(0),
    LastSeenMonth              INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_LastSeenMonth DEFAULT(0),
    ConsecutivePeriods         INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_ConsecutivePeriods DEFAULT(0),
    TotalOccurrences           INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_TotalOccurrences DEFAULT(0),
    IsActive                   BIT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_IsActive DEFAULT(0),
    GeneratedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_GeneratedAt DEFAULT('3000-01-01'),
    CreatedAt                  DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_CreatedAt DEFAULT('3000-01-01'),
    UpdatedAt                  DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId           VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Attention PRIMARY KEY CLUSTERED (EntityAnalyticsAttentionId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Attention_Entity_Signal' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Attention'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Attention_Entity_Signal
    ON BTRPD_EntityAnalytics_Attention (EntityType, EntityId, SignalCode)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Attention_Entity_Active' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Attention'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Attention_Entity_Active
    ON BTRPD_EntityAnalytics_Attention (EntityType, EntityId, IsActive)
    INCLUDE (SignalCode, SignalTitle, FirstSeenYear, FirstSeenMonth, LastSeenYear, LastSeenMonth,
             ConsecutivePeriods, TotalOccurrences, GeneratedAt)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_EntityAnalytics_BackfillCheckpoint.sql
-- ============================================================

CREATE TABLE BTRPD_EntityAnalytics_BackfillCheckpoint
(
    BackfillCheckpointId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_Id DEFAULT(''),
    BackfillJobId        VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_JobId DEFAULT(''),
    EntityType           VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_EntityType DEFAULT(''),
    PeriodYear           INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_PeriodYear DEFAULT(0),
    PeriodMonth          INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_PeriodMonth DEFAULT(0),
    Status               VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_Status DEFAULT(''),
    LayersCompleted      VARCHAR(50)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_LayersCompleted DEFAULT(''),
    EntityCount          INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_EntityCount DEFAULT(0),
    RowCountsJson        VARCHAR(500) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_RowCountsJson DEFAULT(''),
    StartedAt            DATETIME     NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_StartedAt DEFAULT('3000-01-01'),
    CompletedAt          DATETIME     NULL,
    LastError            VARCHAR(500) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_LastError DEFAULT(''),
    LastRefreshLogId     VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_BackfillCheckpoint PRIMARY KEY CLUSTERED (BackfillCheckpointId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_BackfillCheckpoint_Job_Type_Period
    ON BTRPD_EntityAnalytics_BackfillCheckpoint (BackfillJobId, EntityType, PeriodYear, PeriodMonth)
GO

CREATE INDEX IX_BTRPD_EntityAnalytics_BackfillCheckpoint_Job_Status
    ON BTRPD_EntityAnalytics_BackfillCheckpoint (BackfillJobId, EntityType, Status)
GO

CREATE INDEX IX_BTRPD_EntityAnalytics_BackfillCheckpoint_Type_Period_StartedAt
    ON BTRPD_EntityAnalytics_BackfillCheckpoint (EntityType, PeriodYear, PeriodMonth, StartedAt DESC)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_EntityAnalytics_BackfillJob.sql
-- ============================================================

CREATE TABLE BTRPD_EntityAnalytics_BackfillJob
(
    BackfillJobId       VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_Id DEFAULT(''),
    EntityTypeScope     VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_EntityTypeScope DEFAULT(''),
    FromPeriodYear      INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_FromPeriodYear DEFAULT(0),
    FromPeriodMonth     INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_FromPeriodMonth DEFAULT(0),
    ToPeriodYear        INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_ToPeriodYear DEFAULT(0),
    ToPeriodMonth       INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_ToPeriodMonth DEFAULT(0),
    Layers              VARCHAR(50)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_Layers DEFAULT(''),
    OptionsJson         VARCHAR(2000) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_OptionsJson DEFAULT(''),
    Status              VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_Status DEFAULT(''),
    StartedAt           DATETIME     NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_StartedAt DEFAULT('3000-01-01'),
    CompletedAt         DATETIME     NULL,
    TriggeredBy         VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_TriggeredBy DEFAULT(''),
    MachineName         VARCHAR(100) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_MachineName DEFAULT(''),
    LastError           VARCHAR(500) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_LastError DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_BackfillJob PRIMARY KEY CLUSTERED (BackfillJobId)
)
GO

CREATE INDEX IX_BTRPD_EntityAnalytics_BackfillJob_Status_StartedAt
    ON BTRPD_EntityAnalytics_BackfillJob (Status, StartedAt DESC)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_EntityAnalytics_BackfillLock.sql
-- ============================================================

CREATE TABLE BTRPD_EntityAnalytics_BackfillLock
(
    EntityType    VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillLock_EntityType DEFAULT(''),
    BackfillJobId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillLock_JobId DEFAULT(''),
    AcquiredAt    DATETIME    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillLock_AcquiredAt DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTRPD_EntityAnalytics_BackfillLock PRIMARY KEY CLUSTERED (EntityType)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_EntityAnalytics_Current.sql
-- ============================================================

CREATE TABLE BTRPD_EntityAnalytics_Current
(
    EntityAnalyticsCurrentId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_Id DEFAULT(''),
    SnapshotKey              VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_SnapshotKey DEFAULT('CURRENT'),
    EntityType               VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_EntityType DEFAULT(''),
    EntityId                 VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_EntityId DEFAULT(''),
    EntityCode               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_EntityCode DEFAULT(''),
    KpiId                    VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_KpiId DEFAULT(''),
    NumericValue             DECIMAL(18,4)  NULL,
    TextValue                VARCHAR(200)   NULL,
    DefinitionVersion        INT            NULL,
    GeneratedAt              DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_GeneratedAt DEFAULT('3000-01-01'),
    UpdatedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Current PRIMARY KEY CLUSTERED (EntityAnalyticsCurrentId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Current_Entity_Kpi' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Current'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Current_Entity_Kpi
    ON BTRPD_EntityAnalytics_Current (EntityType, EntityId, KpiId, SnapshotKey)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Current_Entity' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Current'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Current_Entity
    ON BTRPD_EntityAnalytics_Current (EntityType, EntityId)
    INCLUDE (KpiId, NumericValue, TextValue, GeneratedAt)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_EntityAnalytics_MonthClose.sql
-- ============================================================

CREATE TABLE BTRPD_EntityAnalytics_MonthClose
(
    EntityAnalyticsMonthCloseId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_Id DEFAULT(''),
    EntityType                  VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_EntityType DEFAULT(''),
    PeriodYear                  INT         NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_PeriodYear DEFAULT(0),
    PeriodMonth                 INT         NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_PeriodMonth DEFAULT(0),
    ClosedAt                    DATETIME    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_ClosedAt DEFAULT('3000-01-01'),
    LastRefreshLogId            VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_MonthClose PRIMARY KEY CLUSTERED (EntityAnalyticsMonthCloseId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_MonthClose_Type_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_MonthClose'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_MonthClose_Type_Period
    ON BTRPD_EntityAnalytics_MonthClose (EntityType, PeriodYear, PeriodMonth)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_EntityAnalytics_Monthly.sql
-- ============================================================

CREATE TABLE BTRPD_EntityAnalytics_Monthly
(
    EntityAnalyticsMonthlyId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_Id DEFAULT(''),
    EntityType               VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_EntityType DEFAULT(''),
    EntityId                 VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_EntityId DEFAULT(''),
    EntityCode               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_EntityCode DEFAULT(''),
    PeriodYear               INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_PeriodYear DEFAULT(0),
    PeriodMonth              INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_PeriodMonth DEFAULT(0),
    KpiId                    VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_KpiId DEFAULT(''),
    NumericValue             DECIMAL(18,4)  NULL,
    TextValue                VARCHAR(200)   NULL,
    PeriodSemantics          VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_PeriodSemantics DEFAULT(''),
    DefinitionVersion        INT            NULL,
    IsClosed                 BIT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_IsClosed DEFAULT(0),
    GeneratedAt              DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_GeneratedAt DEFAULT('3000-01-01'),
    UpdatedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Monthly PRIMARY KEY CLUSTERED (EntityAnalyticsMonthlyId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Monthly_Entity_Period_Kpi' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Monthly'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Monthly_Entity_Period_Kpi
    ON BTRPD_EntityAnalytics_Monthly (EntityType, EntityId, PeriodYear, PeriodMonth, KpiId)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Monthly_Entity_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Monthly'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Monthly_Entity_Period
    ON BTRPD_EntityAnalytics_Monthly (EntityType, EntityId, PeriodYear DESC, PeriodMonth DESC)
    INCLUDE (KpiId, NumericValue, TextValue, PeriodSemantics, IsClosed, GeneratedAt)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Monthly_Type_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Monthly'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Monthly_Type_Period
    ON BTRPD_EntityAnalytics_Monthly (EntityType, PeriodYear, PeriodMonth)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_EntityAnalytics_Radar.sql
-- ============================================================

CREATE TABLE BTRPD_EntityAnalytics_Radar
(
    EntityAnalyticsRadarId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_Id DEFAULT(''),
    EntityType             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_EntityType DEFAULT(''),
    EntityId               VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_EntityId DEFAULT(''),
    EntityCode             VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_EntityCode DEFAULT(''),
    PeriodYear             INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_PeriodYear DEFAULT(0),
    PeriodMonth            INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_PeriodMonth DEFAULT(0),
    AxisKpiId              VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_AxisKpiId DEFAULT(''),
    Score                  DECIMAL(5,2)   NULL,
    PeerGroupRuleId        VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_PeerGroupRuleId DEFAULT(''),
    PeerGroupSize          INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_PeerGroupSize DEFAULT(0),
    NormalizationMethod    VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_NormalizationMethod DEFAULT(''),
    GeneratedAt            DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_GeneratedAt DEFAULT('3000-01-01'),
    UpdatedAt              DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId       VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Radar PRIMARY KEY CLUSTERED (EntityAnalyticsRadarId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Radar_Entity_Period_Axis' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Radar'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Radar_Entity_Period_Axis
    ON BTRPD_EntityAnalytics_Radar (EntityType, EntityId, PeriodYear, PeriodMonth, AxisKpiId)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Radar_Entity_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Radar'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Radar_Entity_Period
    ON BTRPD_EntityAnalytics_Radar (EntityType, EntityId, PeriodYear DESC, PeriodMonth DESC)
    INCLUDE (AxisKpiId, Score, PeerGroupRuleId, PeerGroupSize, NormalizationMethod, GeneratedAt)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Radar_Type_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Radar'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Radar_Type_Period
    ON BTRPD_EntityAnalytics_Radar (EntityType, PeriodYear, PeriodMonth)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_EntityAnalytics_Ranking.sql
-- ============================================================

CREATE TABLE BTRPD_EntityAnalytics_Ranking
(
    EntityAnalyticsRankingId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_Id DEFAULT(''),
    EntityType               VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_EntityType DEFAULT(''),
    EntityId                 VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_EntityId DEFAULT(''),
    EntityCode               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_EntityCode DEFAULT(''),
    PeriodYear               INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_PeriodYear DEFAULT(0),
    PeriodMonth              INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_PeriodMonth DEFAULT(0),
    KpiId                    VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_KpiId DEFAULT(''),
    RankPosition             INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_RankPosition DEFAULT(0),
    PopulationSize           INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_PopulationSize DEFAULT(0),
    Percentile               DECIMAL(5,2)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_Percentile DEFAULT(0),
    GeneratedAt              DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_GeneratedAt DEFAULT('3000-01-01'),
    UpdatedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Ranking PRIMARY KEY CLUSTERED (EntityAnalyticsRankingId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Ranking_Entity_Period_Kpi' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Ranking'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Ranking_Entity_Period_Kpi
    ON BTRPD_EntityAnalytics_Ranking (EntityType, EntityId, PeriodYear, PeriodMonth, KpiId)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Ranking_Entity_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Ranking'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Ranking_Entity_Period
    ON BTRPD_EntityAnalytics_Ranking (EntityType, EntityId, PeriodYear DESC, PeriodMonth DESC)
    INCLUDE (KpiId, RankPosition, PopulationSize, Percentile, GeneratedAt)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Ranking_Type_Period_Kpi' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Ranking'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Ranking_Type_Period_Kpi
    ON BTRPD_EntityAnalytics_Ranking (EntityType, PeriodYear, PeriodMonth, KpiId)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_EntityAnalytics_Relationship.sql
-- ============================================================

CREATE TABLE BTRPD_EntityAnalytics_Relationship
(
    EntityAnalyticsRelationshipId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_Id DEFAULT(''),
    SourceEntityType              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_SourceEntityType DEFAULT(''),
    SourceEntityId                VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_SourceEntityId DEFAULT(''),
    SourceEntityCode              VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_SourceEntityCode DEFAULT(''),
    RelationshipCode              VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_RelationshipCode DEFAULT(''),
    TargetEntityType              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_TargetEntityType DEFAULT(''),
    TargetEntityId                VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_TargetEntityId DEFAULT(''),
    TargetEntityCode              VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_TargetEntityCode DEFAULT(''),
    TargetDisplayName             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_TargetDisplayName DEFAULT(''),
    Rank                          INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_Rank DEFAULT(0),
    MetricValue                   DECIMAL(18,2)  NULL,
    PeriodYear                    INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_PeriodYear DEFAULT(0),
    PeriodMonth                   INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_PeriodMonth DEFAULT(0),
    GeneratedAt                   DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_GeneratedAt DEFAULT('3000-01-01'),
    UpdatedAt                     DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId              VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Relationship PRIMARY KEY CLUSTERED (EntityAnalyticsRelationshipId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Relationship_Source_Relationship_Period_Rank' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Relationship'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Relationship_Source_Relationship_Period_Rank
    ON BTRPD_EntityAnalytics_Relationship (SourceEntityType, SourceEntityId, RelationshipCode, PeriodYear, PeriodMonth, Rank)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Relationship_Source_Relationship' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Relationship'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Relationship_Source_Relationship
    ON BTRPD_EntityAnalytics_Relationship (SourceEntityType, SourceEntityId, RelationshipCode)
    INCLUDE (TargetEntityType, TargetEntityId, TargetEntityCode, TargetDisplayName, Rank, MetricValue,
             PeriodYear, PeriodMonth, GeneratedAt)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_FieldActivityKpi.sql
-- ============================================================

CREATE TABLE BTRPD_FieldActivityKpi
(
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_GeneratedAt DEFAULT('3000-01-01'),
    ActivityDate              DATE          NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_ActivityDate DEFAULT('3000-01-01'),
    ActiveSalesmenCount       INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_ActiveSalesmenCount DEFAULT(0),
    PlannedVisits             INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_PlannedVisits DEFAULT(0),
    ActualVisits              INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_ActualVisits DEFAULT(0),
    VisitExecutionPercent     DECIMAL(9,4)  NULL,
    EffectiveCalls            INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_EffectiveCalls DEFAULT(0),
    EffectiveCallRate         DECIMAL(9,4)  NULL,
    MissedVisits              INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_MissedVisits DEFAULT(0),
    UnplannedVisits           INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_UnplannedVisits DEFAULT(0),
    GpsValidRate              DECIMAL(9,4)  NULL,
    TotalOrders               INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_TotalOrders DEFAULT(0),
    TotalOmzet                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_TotalOmzet DEFAULT(0),
    LastRefreshLogId          VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivityKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_FieldActivityKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_FieldActivitySalesman.sql
-- ============================================================

CREATE TABLE BTRPD_FieldActivitySalesman
(
    SnapshotKey             VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_SnapshotKey DEFAULT('CURRENT'),
    SalesPersonId           VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_SalesPersonId DEFAULT(''),
    SalesPersonCode         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_SalesPersonCode DEFAULT(''),
    SalesPersonName         VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_SalesPersonName DEFAULT(''),
    WilayahName             VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_WilayahName DEFAULT(''),
    HasEmail                BIT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_HasEmail DEFAULT(0),
    Rank                    INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_Rank DEFAULT(0),
    PlannedVisits           INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_PlannedVisits DEFAULT(0),
    ActualVisits            INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_ActualVisits DEFAULT(0),
    VisitExecutionPercent   DECIMAL(9,4)  NULL,
    EffectiveCalls          INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_EffectiveCalls DEFAULT(0),
    EffectiveCallRate       DECIMAL(9,4)  NULL,
    MissedVisits            INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_MissedVisits DEFAULT(0),
    UnplannedVisits         INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_UnplannedVisits DEFAULT(0),
    GpsValidPercent         DECIMAL(9,4)  NULL,
    GpsValidCount           INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_GpsValidCount DEFAULT(0),
    GpsWarningCount         INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_GpsWarningCount DEFAULT(0),
    GpsSuspiciousCount      INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_GpsSuspiciousCount DEFAULT(0),
    OrdersCount             INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_OrdersCount DEFAULT(0),
    OmzetAmount             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_OmzetAmount DEFAULT(0),
    StatusCode              VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivitySalesman_StatusCode DEFAULT(''),

    CONSTRAINT PK_BTRPD_FieldActivitySalesman PRIMARY KEY CLUSTERED (SnapshotKey, SalesPersonId)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_FieldActivityTrend.sql
-- ============================================================

CREATE TABLE BTRPD_FieldActivityTrend
(
    SnapshotKey             VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivityTrend_SnapshotKey DEFAULT('CURRENT'),
    TrendDate               DATE          NOT NULL,
    VisitExecutionPercent   DECIMAL(9,4)  NULL,
    EffectiveCallRate       DECIMAL(9,4)  NULL,
    OrdersCount             INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityTrend_OrdersCount DEFAULT(0),
    OmzetAmount             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_FieldActivityTrend_OmzetAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_FieldActivityTrend PRIMARY KEY CLUSTERED (SnapshotKey, TrendDate)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryBreakdown.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryBreakdown
(
    InventoryBreakdownId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_InventoryBreakdownId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_SnapshotKey DEFAULT('CURRENT'),
    DimensionType        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_DimensionType DEFAULT(''),
    Name                 VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_Name DEFAULT(''),
    SupplierId           VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_SupplierId DEFAULT(''),
    InventoryValue       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_InventoryValue DEFAULT(0),
    IsTop10              BIT           NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_IsTop10 DEFAULT(0),
    Top10Rank            INT           NULL,

    CONSTRAINT PK_BTRPD_InventoryBreakdown PRIMARY KEY CLUSTERED (InventoryBreakdownId)
)
GO

CREATE INDEX IX_BTRPD_InventoryBreakdown_SnapshotKey_DimensionType
    ON BTRPD_InventoryBreakdown (SnapshotKey, DimensionType)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryForecastDailyConsumption.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryForecastDailyConsumption
(
    InventoryForecastDailyConsumptionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_Id DEFAULT(''),
    SnapshotKey                         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_SnapshotKey DEFAULT('CURRENT'),
    ConsumptionDate                     DATETIME      NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_ConsumptionDate DEFAULT('3000-01-01'),
    DayIndex                            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_DayIndex DEFAULT(0),
    UnitsSold                           DECIMAL(18,4) NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_UnitsSold DEFAULT(0),
    AdcReference                        DECIMAL(18,4) NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastDailyConsumption_AdcReference DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryForecastDailyConsumption PRIMARY KEY CLUSTERED (InventoryForecastDailyConsumptionId)
)
GO

CREATE INDEX IX_BTRPD_InventoryForecastDailyConsumption_SnapshotKey_Date
    ON BTRPD_InventoryForecastDailyConsumption (SnapshotKey, ConsumptionDate)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryForecastKpi.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryForecastKpi
(
    SnapshotKey                     VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                       DATETIME       NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    BusinessDate                      DATETIME       NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    PlanningHorizonDays               INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_PlanningHorizonDays DEFAULT(0),
    CurrentInventoryValue             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_CurrentInventoryValue DEFAULT(0),
    ProjectedInventoryValue           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_ProjectedInventoryValue DEFAULT(0),
    BestCaseProjectedValue            DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_BestCaseProjectedValue DEFAULT(0),
    WorstCaseProjectedValue           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_WorstCaseProjectedValue DEFAULT(0),
    AverageDailyConsumptionUnits      DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_AverageDailyConsumptionUnits DEFAULT(0),
    WeightedAverageDaysOfSupply       DECIMAL(18,2)  NULL,
    UnderstockValue                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_UnderstockValue DEFAULT(0),
    OverstockValue                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_OverstockValue DEFAULT(0),
    StockOutRiskItemCount             INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_StockOutRiskItemCount DEFAULT(0),
    InventoryCoveragePercent          DECIMAL(9,4)   NULL,
    InventoryTurnoverForecast         DECIMAL(9,4)   NULL,
    InventoryHealthScore              INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_InventoryHealthScore DEFAULT(0),
    ForecastConfidence                VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_ForecastConfidence DEFAULT(''),
    AtRiskInventoryPercent            DECIMAL(9,4)   NULL,
    ForecastConsumptionUnits          DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_ForecastConsumptionUnits DEFAULT(0),
    HeatCellLowLow                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellLowLow DEFAULT(0),
    HeatCellLowMed                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellLowMed DEFAULT(0),
    HeatCellLowHigh                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellLowHigh DEFAULT(0),
    HeatCellMedLow                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellMedLow DEFAULT(0),
    HeatCellMedMed                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellMedMed DEFAULT(0),
    HeatCellMedHigh                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellMedHigh DEFAULT(0),
    HeatCellHighLow                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellHighLow DEFAULT(0),
    HeatCellHighMed                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellHighMed DEFAULT(0),
    HeatCellHighHigh                  INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_HeatCellHighHigh DEFAULT(0),
    LastRefreshLogId                  VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryForecastLevel.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryForecastLevel
(
    InventoryForecastLevelId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_Id DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_SnapshotKey DEFAULT('CURRENT'),
    HorizonDay               INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_HorizonDay DEFAULT(0),
    ProjectedInventoryValue  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_ProjectedInventoryValue DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryForecastLevel PRIMARY KEY CLUSTERED (InventoryForecastLevelId)
)
GO

CREATE INDEX IX_BTRPD_InventoryForecastLevel_SnapshotKey_Day
    ON BTRPD_InventoryForecastLevel (SnapshotKey, HorizonDay)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryForecastRecommendation.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryForecastRecommendation
(
    InventoryForecastRecommendationId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_Id DEFAULT(''),
    SnapshotKey                       VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                         INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_SortOrder DEFAULT(0),
    BrgId                             VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_BrgId DEFAULT(''),
    BrgCode                           VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_BrgCode DEFAULT(''),
    BrgName                           VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_BrgName DEFAULT(''),
    SupplierName                      VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_SupplierName DEFAULT(''),
    ReorderDate                       DATETIME       NULL,
    RecommendedPurchaseQty            DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_RecommendedPurchaseQty DEFAULT(0),
    AverageDailyConsumption           DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_AverageDailyConsumption DEFAULT(0),
    CurrentQty                        DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_CurrentQty DEFAULT(0),
    DaysOfSupply                      DECIMAL(18,2)  NULL,
    Urgency                           VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_Urgency DEFAULT(''),
    ReportRoute                       VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_ReportRoute DEFAULT(''),
    EntityCode                        VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_EntityCode DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryForecastRecommendation PRIMARY KEY CLUSTERED (InventoryForecastRecommendationId)
)
GO

CREATE INDEX IX_BTRPD_InventoryForecastRecommendation_SnapshotKey_SortOrder
    ON BTRPD_InventoryForecastRecommendation (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryForecastRisk.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryForecastRisk
(
    InventoryForecastRiskId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_Id DEFAULT(''),
    SnapshotKey             VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SnapshotKey DEFAULT('CURRENT'),
    SortOrder               INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SortOrder DEFAULT(0),
    SignalKey               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SignalKey DEFAULT(''),
    SignalLabel             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SignalLabel DEFAULT(''),
    BrgId                   VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_BrgId DEFAULT(''),
    BrgCode                 VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_BrgCode DEFAULT(''),
    BrgName                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_BrgName DEFAULT(''),
    SupplierName            VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SupplierName DEFAULT(''),
    DaysOfSupply            DECIMAL(18,2)  NULL,
    StockOutDate            DATETIME       NULL,
    ValueAmount             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_ValueAmount DEFAULT(0),
    Urgency                 VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_Urgency DEFAULT(''),
    RuleExplanation         VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_RuleExplanation DEFAULT(''),
    ReportRoute             VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_ReportRoute DEFAULT(''),
    EntityCode              VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_EntityCode DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryForecastRisk PRIMARY KEY CLUSTERED (InventoryForecastRiskId)
)
GO

CREATE INDEX IX_BTRPD_InventoryForecastRisk_SnapshotKey_SortOrder
    ON BTRPD_InventoryForecastRisk (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryKpi.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryKpi
(
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt          DATETIME      NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalInventoryValue  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_TotalInventoryValue DEFAULT(0),
    TotalItem            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_TotalItem DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryOptimizationAction.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryOptimizationAction
(
    InventoryOptimizationActionId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_Id DEFAULT(''),
    SnapshotKey                   VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                     INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_SortOrder DEFAULT(0),
    PriorityScore                 INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_PriorityScore DEFAULT(0),
    Category                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_Category DEFAULT(''),
    ActionType                    VARCHAR(40)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ActionType DEFAULT(''),
    ActionLabel                   VARCHAR(80)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ActionLabel DEFAULT(''),
    BrgId                         VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_BrgId DEFAULT(''),
    BrgName                       VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_BrgName DEFAULT(''),
    SupplierName                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_SupplierName DEFAULT(''),
    WarehouseFromId               VARCHAR(5)     NULL,
    WarehouseFromName             VARCHAR(50)    NULL,
    WarehouseToId                 VARCHAR(5)     NULL,
    WarehouseToName               VARCHAR(50)    NULL,
    Quantity                      DECIMAL(18,4)  NULL,
    ImpactValueIdr                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ImpactValueIdr DEFAULT(0),
    DaysOfSupply                  DECIMAL(18,2)  NULL,
    ReasonText                    VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ReasonText DEFAULT(''),
    RuleId                        VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_RuleId DEFAULT(''),
    ReportRoute                   VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_ReportRoute DEFAULT(''),
    DrillDownRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationAction_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationAction PRIMARY KEY CLUSTERED (InventoryOptimizationActionId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationAction_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationAction (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryOptimizationActionHeat.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryOptimizationActionHeat
(
    InventoryOptimizationActionHeatId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_Id DEFAULT(''),
    SnapshotKey                       VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_SnapshotKey DEFAULT('CURRENT'),
    ActionType                        VARCHAR(40) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_ActionType DEFAULT(''),
    ActionLabel                       VARCHAR(80) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_ActionLabel DEFAULT(''),
    Category                          VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_Category DEFAULT(''),
    ActionCount                       INT         NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationActionHeat_ActionCount DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryOptimizationActionHeat PRIMARY KEY CLUSTERED (InventoryOptimizationActionHeatId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationActionHeat_SnapshotKey
    ON BTRPD_InventoryOptimizationActionHeat (SnapshotKey)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryOptimizationClearance.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryOptimizationClearance
(
    InventoryOptimizationClearanceId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_SortOrder DEFAULT(0),
    PriorityScore                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_PriorityScore DEFAULT(0),
    Category                         VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_Category DEFAULT(''),
    BrgId                            VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_BrgId DEFAULT(''),
    BrgName                          VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_BrgName DEFAULT(''),
    InventoryValueIdr                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_InventoryValueIdr DEFAULT(0),
    IdleDays                         INT            NULL,
    RecommendedAction                VARCHAR(80)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_RecommendedAction DEFAULT(''),
    ReasonText                       VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_ReasonText DEFAULT(''),
    RuleId                           VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_RuleId DEFAULT(''),
    ReportRoute                      VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationClearance_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationClearance PRIMARY KEY CLUSTERED (InventoryOptimizationClearanceId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationClearance_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationClearance (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryOptimizationDelay.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryOptimizationDelay
(
    InventoryOptimizationDelayId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_Id DEFAULT(''),
    SnapshotKey                  VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_SortOrder DEFAULT(0),
    PriorityScore                INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_PriorityScore DEFAULT(0),
    Category                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_Category DEFAULT(''),
    ActionType                   VARCHAR(40)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ActionType DEFAULT(''),
    ActionLabel                  VARCHAR(80)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ActionLabel DEFAULT(''),
    BrgId                        VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_BrgId DEFAULT(''),
    BrgName                      VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_BrgName DEFAULT(''),
    SupplierName                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_SupplierName DEFAULT(''),
    DaysOfSupply                 DECIMAL(18,2)  NULL,
    MovementClass                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_MovementClass DEFAULT(''),
    SuggestedQty                 DECIMAL(18,4)  NULL,
    ReasonText                   VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ReasonText DEFAULT(''),
    RuleId                       VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_RuleId DEFAULT(''),
    ReportRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_ReportRoute DEFAULT(''),
    DrillDownRoute               VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationDelay_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationDelay PRIMARY KEY CLUSTERED (InventoryOptimizationDelayId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationDelay_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationDelay (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryOptimizationKpi.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryOptimizationKpi
(
    SnapshotKey                     VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                       DATETIME       NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_GeneratedAt DEFAULT('3000-01-01'),
    BusinessDate                      DATETIME       NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_BusinessDate DEFAULT('3000-01-01'),
    PlanningHorizonDays               INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_PlanningHorizonDays DEFAULT(0),
    BudgetCapIdr                      DECIMAL(18,2)  NULL,
    InventoryHealthScore              INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_InventoryHealthScore DEFAULT(0),
    CriticalActionCount               INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_CriticalActionCount DEFAULT(0),
    HighActionCount                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_HighActionCount DEFAULT(0),
    MediumActionCount                 INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_MediumActionCount DEFAULT(0),
    LowActionCount                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_LowActionCount DEFAULT(0),
    PurchaseNowCount                  INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_PurchaseNowCount DEFAULT(0),
    DelayCount                        INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_DelayCount DEFAULT(0),
    TransferCount                     INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_TransferCount DEFAULT(0),
    ClearanceCount                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_ClearanceCount DEFAULT(0),
    PostFirstCount                    INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_PostFirstCount DEFAULT(0),
    DeferCount                        INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_DeferCount DEFAULT(0),
    RequiredPurchaseBudgetIdr         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_RequiredPurchaseBudgetIdr DEFAULT(0),
    RecommendedPurchaseBudgetIdr      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_RecommendedPurchaseBudgetIdr DEFAULT(0),
    DeferrableSpendIdr                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_DeferrableSpendIdr DEFAULT(0),
    RecoverableCapitalIdr             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_RecoverableCapitalIdr DEFAULT(0),
    LastRefreshLogId                  VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryOptimizationPriorityDist.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryOptimizationPriorityDist
(
    InventoryOptimizationPriorityDistId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_Id DEFAULT(''),
    SnapshotKey                         VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_SnapshotKey DEFAULT('CURRENT'),
    Category                            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_Category DEFAULT(''),
    ActionCount                         INT         NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_ActionCount DEFAULT(0),
    SortOrder                           INT         NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationPriorityDist_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryOptimizationPriorityDist PRIMARY KEY CLUSTERED (InventoryOptimizationPriorityDistId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationPriorityDist_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationPriorityDist (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryOptimizationReorder.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryOptimizationReorder
(
    InventoryOptimizationReorderId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_Id DEFAULT(''),
    SnapshotKey                    VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                      INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_SortOrder DEFAULT(0),
    PriorityScore                  INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_PriorityScore DEFAULT(0),
    Category                       VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_Category DEFAULT(''),
    BrgId                          VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_BrgId DEFAULT(''),
    BrgCode                        VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_BrgCode DEFAULT(''),
    BrgName                        VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_BrgName DEFAULT(''),
    SupplierName                   VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_SupplierName DEFAULT(''),
    RecommendedPurchaseQty         DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_RecommendedPurchaseQty DEFAULT(0),
    EstimatedCostIdr               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_EstimatedCostIdr DEFAULT(0),
    DaysOfSupply                   DECIMAL(18,2)  NULL,
    ReorderDate                    DATETIME       NULL,
    AverageDailyConsumption        DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_AverageDailyConsumption DEFAULT(0),
    CurrentQty                     DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_CurrentQty DEFAULT(0),
    ReasonText                     VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_ReasonText DEFAULT(''),
    RuleId                         VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_RuleId DEFAULT(''),
    ReportRoute                    VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_ReportRoute DEFAULT(''),
    DrillDownRoute                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationReorder_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationReorder PRIMARY KEY CLUSTERED (InventoryOptimizationReorderId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationReorder_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationReorder (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryOptimizationTransfer.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryOptimizationTransfer
(
    InventoryOptimizationTransferId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_Id DEFAULT(''),
    SnapshotKey                     VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                       INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_SortOrder DEFAULT(0),
    PriorityScore                   INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_PriorityScore DEFAULT(0),
    Category                        VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_Category DEFAULT(''),
    BrgId                           VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_BrgId DEFAULT(''),
    BrgName                         VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_BrgName DEFAULT(''),
    WarehouseFromId                 VARCHAR(5)     NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseFromId DEFAULT(''),
    WarehouseFromName               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseFromName DEFAULT(''),
    WarehouseToId                   VARCHAR(5)     NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseToId DEFAULT(''),
    WarehouseToName                 VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_WarehouseToName DEFAULT(''),
    TransferQty                     DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_TransferQty DEFAULT(0),
    DestDaysOfSupply                DECIMAL(18,2)  NULL,
    ReasonText                      VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_ReasonText DEFAULT(''),
    RuleId                          VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_RuleId DEFAULT(''),
    ReportRoute                     VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_ReportRoute DEFAULT(''),
    DrillDownRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryOptimizationTransfer_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryOptimizationTransfer PRIMARY KEY CLUSTERED (InventoryOptimizationTransferId)
)
GO

CREATE INDEX IX_BTRPD_InventoryOptimizationTransfer_SnapshotKey_SortOrder
    ON BTRPD_InventoryOptimizationTransfer (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryRiskAging.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryRiskAging
(
    InventoryRiskAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_InventoryRiskAgingId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_SnapshotKey DEFAULT('CURRENT'),
    BucketKey            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_BucketKey DEFAULT(''),
    BucketLabel          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_BucketLabel DEFAULT(''),
    InventoryValue       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_InventoryValue DEFAULT(0),
    ItemCount            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_ItemCount DEFAULT(0),
    SortOrder            INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAging_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryRiskAging PRIMARY KEY CLUSTERED (InventoryRiskAgingId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskAging_SnapshotKey_BucketKey
    ON BTRPD_InventoryRiskAging (SnapshotKey, BucketKey)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryRiskAttention.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryRiskAttention
(
    InventoryRiskAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_InventoryRiskAttentionId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SnapshotKey DEFAULT('CURRENT'),
    BrgId                    VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_BrgId DEFAULT(''),
    BrgCode                  VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_BrgCode DEFAULT(''),
    BrgName                  VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_BrgName DEFAULT(''),
    KategoriName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_KategoriName DEFAULT(''),
    SupplierName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SupplierName DEFAULT(''),
    Qty                      INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_Qty DEFAULT(0),
    InventoryValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur      INT           NULL,
    SignalKey                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SignalKey DEFAULT(''),
    SignalLabel              VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SignalLabel DEFAULT(''),
    SortOrder                INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryRiskAttention PRIMARY KEY CLUSTERED (InventoryRiskAttentionId)
)
GO

CREATE INDEX IX_BTRPD_InventoryRiskAttention_SnapshotKey_SortOrder
    ON BTRPD_InventoryRiskAttention (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryRiskBreakdown.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryRiskBreakdown
(
    InventoryRiskBreakdownId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_InventoryRiskBreakdownId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_SnapshotKey DEFAULT('CURRENT'),
    DimensionType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_DimensionType DEFAULT(''),
    Name                     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_Name DEFAULT(''),
    SupplierId               VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_SupplierId DEFAULT(''),
    AtRiskValue              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_AtRiskValue DEFAULT(0),
    ItemCount                INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_ItemCount DEFAULT(0),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_Rank DEFAULT(0),
    PercentOfAtRisk          DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_InventoryRiskBreakdown PRIMARY KEY CLUSTERED (InventoryRiskBreakdownId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskBreakdown_SnapshotKey_DimensionType_Rank
    ON BTRPD_InventoryRiskBreakdown (SnapshotKey, DimensionType, Rank)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryRiskKpi.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryRiskKpi
(
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt              DATETIME      NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalInventoryValue      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_TotalInventoryValue DEFAULT(0),
    TotalItem                INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_TotalItem DEFAULT(0),
    DeadStockItemCount       INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_DeadStockItemCount DEFAULT(0),
    DeadStockValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_DeadStockValue DEFAULT(0),
    SlowMovingItemCount      INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_SlowMovingItemCount DEFAULT(0),
    SlowMovingValue          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_SlowMovingValue DEFAULT(0),
    NeverSoldItemCount       INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_NeverSoldItemCount DEFAULT(0),
    NeverSoldValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_NeverSoldValue DEFAULT(0),
    AtRiskInventoryValue     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_AtRiskInventoryValue DEFAULT(0),
    AtRiskInventoryPercent   DECIMAL(9,4)  NULL,
    RequiresAttention        BIT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_RequiresAttention DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryRiskKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryRiskTopDead.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryRiskTopDead
(
    InventoryRiskTopDeadId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_InventoryRiskTopDeadId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_SnapshotKey DEFAULT('CURRENT'),
    Rank                   INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_Rank DEFAULT(0),
    BrgId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_BrgId DEFAULT(''),
    BrgCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_BrgCode DEFAULT(''),
    BrgName                VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_BrgName DEFAULT(''),
    KategoriName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_KategoriName DEFAULT(''),
    SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_SupplierName DEFAULT(''),
    Qty                    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_Qty DEFAULT(0),
    InventoryValue         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_DaysSinceLastFaktur DEFAULT(0),
    PercentOfAtRisk        DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_InventoryRiskTopDead PRIMARY KEY CLUSTERED (InventoryRiskTopDeadId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskTopDead_SnapshotKey_Rank
    ON BTRPD_InventoryRiskTopDead (SnapshotKey, Rank)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_InventoryRiskTopSlow.sql
-- ============================================================

CREATE TABLE BTRPD_InventoryRiskTopSlow
(
    InventoryRiskTopSlowId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_InventoryRiskTopSlowId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_SnapshotKey DEFAULT('CURRENT'),
    Rank                   INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_Rank DEFAULT(0),
    BrgId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_BrgId DEFAULT(''),
    BrgCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_BrgCode DEFAULT(''),
    BrgName                VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_BrgName DEFAULT(''),
    KategoriName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_KategoriName DEFAULT(''),
    SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_SupplierName DEFAULT(''),
    Qty                    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_Qty DEFAULT(0),
    InventoryValue         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_DaysSinceLastFaktur DEFAULT(0),
    PercentOfAtRisk        DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_InventoryRiskTopSlow PRIMARY KEY CLUSTERED (InventoryRiskTopSlowId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskTopSlow_SnapshotKey_Rank
    ON BTRPD_InventoryRiskTopSlow (SnapshotKey, Rank)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_LocationAttention.sql
-- ============================================================

CREATE TABLE BTRPD_LocationAttention

(

    LocationAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_LocationAttentionId DEFAULT(''),

    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SnapshotKey DEFAULT('CURRENT'),

    EntityType          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_EntityType DEFAULT(''),

    EntityCode          VARCHAR(5)    NULL,

    EntityName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_EntityName DEFAULT(''),

    SignalKey           VARCHAR(40)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SignalKey DEFAULT(''),

    SignalLabel         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SignalLabel DEFAULT(''),

    ValueAmount         DECIMAL(18,2) NULL,

    ValueText           VARCHAR(100)  NULL,

    ReportRoute         VARCHAR(100)  NULL,

    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SortOrder DEFAULT(0),



    CONSTRAINT PK_BTRPD_LocationAttention PRIMARY KEY CLUSTERED (LocationAttentionId)

)

GO



CREATE INDEX IX_BTRPD_LocationAttention_SnapshotKey_SortOrder

    ON BTRPD_LocationAttention (SnapshotKey, SortOrder)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_LocationKpi.sql
-- ============================================================

CREATE TABLE BTRPD_LocationKpi

(

    SnapshotKey                        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_SnapshotKey DEFAULT('CURRENT'),

    GeneratedAt                        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_GeneratedAt DEFAULT('3000-01-01'),

    PeriodYear                         INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_PeriodYear DEFAULT(0),

    PeriodMonth                        INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_PeriodMonth DEFAULT(0),

    Top1WarehouseInventoryPercent      DECIMAL(9,4)  NULL,

    Top3WarehouseInventoryPercent      DECIMAL(9,4)  NULL,

    Top1WarehouseAtRiskPercent         DECIMAL(9,4)  NULL,

    Top1WarehouseSalesPercent          DECIMAL(9,4)  NULL,

    Top1WilayahSalesPercent            DECIMAL(9,4)  NULL,

    InactiveWarehouseWithStockCount    INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_InactiveWarehouseWithStockCount DEFAULT(0),

    WarehouseNoSalesWithInventoryCount INT           NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_WarehouseNoSalesWithInventoryCount DEFAULT(0),

    TotalInventoryValue                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalInventoryValue DEFAULT(0),

    TotalAtRiskValue                   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalAtRiskValue DEFAULT(0),

    TotalOmzet                         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalOmzet DEFAULT(0),

    TotalPurchase                      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_TotalPurchase DEFAULT(0),

    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationKpi_LastRefreshLogId DEFAULT(''),



    CONSTRAINT PK_BTRPD_LocationKpi PRIMARY KEY CLUSTERED (SnapshotKey)

)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_LocationTopWarehouseAtRisk.sql
-- ============================================================

CREATE TABLE BTRPD_LocationTopWarehouseAtRisk

(

    LocationTopWarehouseAtRiskId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_LocationTopWarehouseAtRiskId DEFAULT(''),

    SnapshotKey                  VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_SnapshotKey DEFAULT('CURRENT'),

    Rank                         INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_Rank DEFAULT(0),

    WarehouseId                  VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_WarehouseId DEFAULT(''),

    WarehouseName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_WarehouseName DEFAULT(''),

    AtRiskValue                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseAtRisk_AtRiskValue DEFAULT(0),

    PercentOfTotal               DECIMAL(9,4)  NULL,

    ReportRoute                  VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehouseAtRisk PRIMARY KEY CLUSTERED (LocationTopWarehouseAtRiskId),

    CONSTRAINT UX_BTRPD_LocationTopWarehouseAtRisk_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_LocationTopWarehouseInventory.sql
-- ============================================================

CREATE TABLE BTRPD_LocationTopWarehouseInventory

(

    LocationTopWarehouseInventoryId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_LocationTopWarehouseInventoryId DEFAULT(''),

    SnapshotKey                     VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_SnapshotKey DEFAULT('CURRENT'),

    Rank                            INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_Rank DEFAULT(0),

    WarehouseId                     VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_WarehouseId DEFAULT(''),

    WarehouseName                   VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_WarehouseName DEFAULT(''),

    InventoryValue                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseInventory_InventoryValue DEFAULT(0),

    PercentOfTotal                  DECIMAL(9,4)  NULL,

    ReportRoute                     VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehouseInventory PRIMARY KEY CLUSTERED (LocationTopWarehouseInventoryId),

    CONSTRAINT UX_BTRPD_LocationTopWarehouseInventory_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_LocationTopWarehousePurchasing.sql
-- ============================================================

CREATE TABLE BTRPD_LocationTopWarehousePurchasing

(

    LocationTopWarehousePurchasingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_LocationTopWarehousePurchasingId DEFAULT(''),

    SnapshotKey                      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_SnapshotKey DEFAULT('CURRENT'),

    Rank                             INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_Rank DEFAULT(0),

    WarehouseId                      VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_WarehouseId DEFAULT(''),

    WarehouseName                    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_WarehouseName DEFAULT(''),

    MtdPurchaseAmount                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_MtdPurchaseAmount DEFAULT(0),

    PercentOfTotal                   DECIMAL(9,4)  NULL,

    ReportRoute                      VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehousePurchasing PRIMARY KEY CLUSTERED (LocationTopWarehousePurchasingId),

    CONSTRAINT UX_BTRPD_LocationTopWarehousePurchasing_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_LocationTopWarehouseSales.sql
-- ============================================================

CREATE TABLE BTRPD_LocationTopWarehouseSales

(

    LocationTopWarehouseSalesId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_LocationTopWarehouseSalesId DEFAULT(''),

    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_SnapshotKey DEFAULT('CURRENT'),

    Rank                        INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_Rank DEFAULT(0),

    WarehouseId                 VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_WarehouseId DEFAULT(''),

    WarehouseName               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_WarehouseName DEFAULT(''),

    MtdOmzet                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_MtdOmzet DEFAULT(0),

    PercentOfTotal              DECIMAL(9,4)  NULL,

    ReportRoute                 VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehouseSales PRIMARY KEY CLUSTERED (LocationTopWarehouseSalesId),

    CONSTRAINT UX_BTRPD_LocationTopWarehouseSales_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_LocationTopWilayahSales.sql
-- ============================================================

CREATE TABLE BTRPD_LocationTopWilayahSales

(

    LocationTopWilayahSalesId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_LocationTopWilayahSalesId DEFAULT(''),

    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_SnapshotKey DEFAULT('CURRENT'),

    Rank                      INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_Rank DEFAULT(0),

    WilayahId                 VARCHAR(5)    NULL,

    WilayahName               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_WilayahName DEFAULT(''),

    MtdOmzet                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_MtdOmzet DEFAULT(0),

    PercentOfTotal            DECIMAL(9,4)  NULL,

    DashboardRoute            VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWilayahSales PRIMARY KEY CLUSTERED (LocationTopWilayahSalesId),

    CONSTRAINT UX_BTRPD_LocationTopWilayahSales_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)

GO



-- ============================================================
-- File: \ReportingContext\BTRPD_PiutangAging.sql
-- ============================================================

CREATE TABLE BTRPD_PiutangAging
(
    PiutangAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_PiutangAgingId DEFAULT(''),
    SnapshotKey    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_SnapshotKey DEFAULT('CURRENT'),
    BucketKey      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_BucketKey DEFAULT(''),
    BucketLabel    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_BucketLabel DEFAULT(''),
    SortOrder      INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_SortOrder DEFAULT(0),
    Amount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangAging_Amount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PiutangAging PRIMARY KEY CLUSTERED (PiutangAgingId),
    CONSTRAINT UX_BTRPD_PiutangAging_SnapshotKey_BucketKey UNIQUE (SnapshotKey, BucketKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PiutangCustomerAging.sql
-- ============================================================

CREATE TABLE BTRPD_PiutangCustomerAging
(
    PiutangCustomerAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_PiutangCustomerAgingId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_SnapshotKey DEFAULT('CURRENT'),
    CustomerId             VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerId DEFAULT(''),
    CustomerCode           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerCode DEFAULT(''),
    CustomerName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerName DEFAULT(''),
    CurrentAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CurrentAmount DEFAULT(0),
    Aging30Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging30Amount DEFAULT(0),
    Aging60Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging60Amount DEFAULT(0),
    Aging90Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging90Amount DEFAULT(0),
    AgingOver90Amount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_AgingOver90Amount DEFAULT(0),
    LastUpdate             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_LastUpdate DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTRPD_PiutangCustomerAging PRIMARY KEY CLUSTERED (PiutangCustomerAgingId),
    CONSTRAINT UX_BTRPD_PiutangCustomerAging_SnapshotKey_CustomerId UNIQUE (SnapshotKey, CustomerId)
)
GO

CREATE INDEX IX_BTRPD_PiutangCustomerAging_SnapshotKey
    ON BTRPD_PiutangCustomerAging (SnapshotKey)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PiutangKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PiutangKpi
(
    SnapshotKey                         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                         DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalPiutang                        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_TotalPiutang DEFAULT(0),
    TotalCustomer                       INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_TotalCustomer DEFAULT(0),
    OverdueCustomer                     INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_OverdueCustomer DEFAULT(0),
    OverduePiutang                      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_OverduePiutang DEFAULT(0),
    AgingOver90Amount                   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_AgingOver90Amount DEFAULT(0),
    AgingOver90Percent                  DECIMAL(9,4)  NULL,
    Top10CustomerConcentrationPercent   DECIMAL(9,4)  NULL,
    Top20CustomerConcentrationPercent   DECIMAL(9,4)  NULL,
    LastRefreshLogId                    VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PiutangKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PiutangTopCustomer.sql
-- ============================================================

CREATE TABLE BTRPD_PiutangTopCustomer
(
    PiutangTopCustomerId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_PiutangTopCustomerId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_Rank DEFAULT(0),
    CustomerName         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_CustomerName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_OutstandingBalance DEFAULT(0),

    CONSTRAINT PK_BTRPD_PiutangTopCustomer PRIMARY KEY CLUSTERED (PiutangTopCustomerId),
    CONSTRAINT UX_BTRPD_PiutangTopCustomer_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PiutangTopCustomerRisk.sql
-- ============================================================

CREATE TABLE BTRPD_PiutangTopCustomerRisk
(
    PiutangTopCustomerRiskId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_PiutangTopCustomerRiskId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Rank DEFAULT(0),
    CustomerId               VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerId DEFAULT(''),
    CustomerCode             VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerCode DEFAULT(''),
    CustomerName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerName DEFAULT(''),
    TotalPiutang             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_TotalPiutang DEFAULT(0),
    CurrentAmount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CurrentAmount DEFAULT(0),
    Aging30Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging30Amount DEFAULT(0),
    Aging60Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging60Amount DEFAULT(0),
    Aging90Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging90Amount DEFAULT(0),
    AgingOver90Amount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_AgingOver90Amount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PiutangTopCustomerRisk PRIMARY KEY CLUSTERED (PiutangTopCustomerRiskId),
    CONSTRAINT UX_BTRPD_PiutangTopCustomerRisk_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalAchievement.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalAchievement
(
    PrincipalAchievementId   VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_PrincipalAchievementId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_SnapshotKey DEFAULT('CURRENT'),
    AchievementAmountKpiId   VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_AchievementAmountKpiId DEFAULT('PRN-TGT-002'),
    AchievementPercentageKpiId VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_AchievementPercentageKpiId DEFAULT('PRN-TGT-003'),
    SalesOutKpiId            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    TargetKpiId              VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_TargetKpiId DEFAULT('PRN-TGT-001'),
    PeriodYear               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_PeriodYear DEFAULT(0),
    PeriodMonth              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_PeriodMonth DEFAULT(0),
    SupplierId               VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_SupplierId DEFAULT(''),
    SupplierName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_SupplierName DEFAULT(''),
    SalesOutAmount           DECIMAL(18,2) NULL,
    TargetAmount             DECIMAL(18,2) NULL,
    AchievementAmount        DECIMAL(18,2) NULL,
    AchievementPercentage    DECIMAL(18,6) NULL,
    SortOrder                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_SortOrder DEFAULT(0),
    GeneratedAt              DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalAchievement PRIMARY KEY CLUSTERED (PrincipalAchievementId),
    CONSTRAINT UX_BTRPD_PrincipalAchievement_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalAchievement_SnapshotKey_SortOrder
    ON BTRPD_PrincipalAchievement (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalAchievementKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalAchievementKpi
(
    SnapshotKey                VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_SnapshotKey DEFAULT('CURRENT'),
    AchievementAmountKpiId     VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_AchievementAmountKpiId DEFAULT('PRN-TGT-002'),
    AchievementPercentageKpiId VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_AchievementPercentageKpiId DEFAULT('PRN-TGT-003'),
    SalesOutKpiId              VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    TargetKpiId                VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_TargetKpiId DEFAULT('PRN-TGT-001'),
    PeriodYear                 INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_PeriodYear DEFAULT(0),
    PeriodMonth                INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_PeriodMonth DEFAULT(0),
    GeneratedAt                DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId           VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalAchievementKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalActiveCustomer.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalActiveCustomer
(
    PrincipalActiveCustomerId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_PrincipalActiveCustomerId DEFAULT(''),
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SnapshotKey DEFAULT('CURRENT'),
    ActiveCustomerKpiId       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_ActiveCustomerKpiId DEFAULT('PRN-CUS-001'),
    AsOfDate                  DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_AsOfDate DEFAULT('3000-01-01'),
    SupplierId                VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SupplierId DEFAULT(''),
    SupplierName              VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SupplierName DEFAULT(''),
    ActiveCustomerCount       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_ActiveCustomerCount DEFAULT(0),
    SortOrder                 INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SortOrder DEFAULT(0),
    GeneratedAt               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId          VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalActiveCustomer PRIMARY KEY CLUSTERED (PrincipalActiveCustomerId),
    CONSTRAINT UX_BTRPD_PrincipalActiveCustomer_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalActiveCustomer_SnapshotKey_SortOrder
    ON BTRPD_PrincipalActiveCustomer (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalActiveCustomerKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalActiveCustomerKpi
(
    SnapshotKey            VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_SnapshotKey DEFAULT('CURRENT'),
    ActiveCustomerKpiId    VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_ActiveCustomerKpiId DEFAULT('PRN-CUS-001'),
    AsOfDate               DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_AsOfDate DEFAULT('3000-01-01'),
    GeneratedAt            DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId       VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalActiveCustomerKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalContribution.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalContribution
(
    PrincipalContributionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_PrincipalContributionId DEFAULT(''),
    SnapshotKey             VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SnapshotKey DEFAULT('CURRENT'),
    SourceSalesOutKpiId     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SourceSalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_PeriodYear DEFAULT(0),
    PeriodMonth             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_PeriodMonth DEFAULT(0),
    SupplierId              VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SupplierId DEFAULT(''),
    SupplierName            VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SupplierName DEFAULT(''),
    SalesPersonId           VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SalesPersonId DEFAULT(''),
    SalesPersonCode         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SalesPersonCode DEFAULT(''),
    SalesPersonName         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SalesPersonName DEFAULT(''),
    ContributionAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_ContributionAmount DEFAULT(0),
    LineCount               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_LineCount DEFAULT(0),
    HasTargetResponsibility BIT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_HasTargetResponsibility DEFAULT(0),
    SortOrder               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SortOrder DEFAULT(0),
    GeneratedAt             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId        VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalContribution PRIMARY KEY CLUSTERED (PrincipalContributionId),
    CONSTRAINT UX_BTRPD_PrincipalContribution_SnapshotKey_SupplierId_SalesPersonId UNIQUE (SnapshotKey, SupplierId, SalesPersonId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalContribution_SnapshotKey_SortOrder
    ON BTRPD_PrincipalContribution (SnapshotKey, SortOrder)
GO

CREATE INDEX IX_BTRPD_PrincipalContribution_SnapshotKey_SupplierId
    ON BTRPD_PrincipalContribution (SnapshotKey, SupplierId)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalContributionException.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalContributionException
(
    PrincipalContributionExceptionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_PrincipalContributionExceptionId DEFAULT(''),
    SnapshotKey                      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SnapshotKey DEFAULT('CURRENT'),
    PeriodYear                       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_PeriodYear DEFAULT(0),
    PeriodMonth                      INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_PeriodMonth DEFAULT(0),
    SupplierId                       VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SupplierId DEFAULT(''),
    SupplierName                     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SupplierName DEFAULT(''),
    SalesPersonId                    VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SalesPersonId DEFAULT(''),
    SalesPersonCode                  VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SalesPersonCode DEFAULT(''),
    SalesPersonName                  VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SalesPersonName DEFAULT(''),
    ContributionAmount               DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_ContributionAmount DEFAULT(0),
    LineCount                        INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_LineCount DEFAULT(0),
    TargetYear                       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_TargetYear DEFAULT(0),
    TargetMonth                      INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_TargetMonth DEFAULT(0),
    SortOrder                        INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SortOrder DEFAULT(0),
    GeneratedAt                      DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId                 VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalContributionException PRIMARY KEY CLUSTERED (PrincipalContributionExceptionId),
    CONSTRAINT UX_BTRPD_PrincipalContributionException_SnapshotKey_SupplierId_SalesPersonId UNIQUE (SnapshotKey, SupplierId, SalesPersonId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalContributionException_SnapshotKey_SortOrder
    ON BTRPD_PrincipalContributionException (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalContributionKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalContributionKpi
(
    SnapshotKey          VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_SnapshotKey DEFAULT('CURRENT'),
    SourceSalesOutKpiId  VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_SourceSalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear           INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_PeriodYear DEFAULT(0),
    PeriodMonth          INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_PeriodMonth DEFAULT(0),
    GeneratedAt          DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId     VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalContributionKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalCustomerCoverage.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalCustomerCoverage
(
    PrincipalCustomerCoverageId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_PrincipalCustomerCoverageId DEFAULT(''),
    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_SnapshotKey DEFAULT('CURRENT'),
    CustomerCoverageKpiId       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_CustomerCoverageKpiId DEFAULT('PRN-CUS-002'),
    AsOfDate                    DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_AsOfDate DEFAULT('3000-01-01'),
    SupplierId                  VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_SupplierId DEFAULT(''),
    SupplierName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_SupplierName DEFAULT(''),
    ActiveCustomerCount         INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_ActiveCustomerCount DEFAULT(0),
    TotalCustomerCount          INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_TotalCustomerCount DEFAULT(0),
    CoveragePercentage          DECIMAL(18,6) NULL,
    SortOrder                   INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_SortOrder DEFAULT(0),
    GeneratedAt                 DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId            VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalCustomerCoverage PRIMARY KEY CLUSTERED (PrincipalCustomerCoverageId),
    CONSTRAINT UX_BTRPD_PrincipalCustomerCoverage_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalCustomerCoverage_SnapshotKey_SortOrder
    ON BTRPD_PrincipalCustomerCoverage (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalCustomerCoverageKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalCustomerCoverageKpi
(
    SnapshotKey            VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_SnapshotKey DEFAULT('CURRENT'),
    CustomerCoverageKpiId  VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_CustomerCoverageKpiId DEFAULT('PRN-CUS-002'),
    AsOfDate               DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_AsOfDate DEFAULT('3000-01-01'),
    GeneratedAt            DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId       VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalCustomerCoverageKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalInventory.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalInventory
(
    PrincipalInventoryId  VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_PrincipalInventoryId DEFAULT(''),
    SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_SnapshotKey DEFAULT('CURRENT'),
    InventoryValueKpiId   VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_InventoryValueKpiId DEFAULT('PRN-INV-001'),
    InventoryDaysKpiId    VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_InventoryDaysKpiId DEFAULT('PRN-INV-002'),
    SupplierId            VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_SupplierId DEFAULT(''),
    SupplierName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_SupplierName DEFAULT(''),
    InventoryValue        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_InventoryValue DEFAULT(0),
    InventoryDays         DECIMAL(18,2) NULL,
    ItemCount             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_ItemCount DEFAULT(0),
    SortOrder             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_SortOrder DEFAULT(0),
    BusinessDate          DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_BusinessDate DEFAULT('3000-01-01'),
    GeneratedAt           DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId      VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventory_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalInventory PRIMARY KEY CLUSTERED (PrincipalInventoryId),
    CONSTRAINT UX_BTRPD_PrincipalInventory_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalInventory_SnapshotKey_SortOrder
    ON BTRPD_PrincipalInventory (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalInventoryKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalInventoryKpi
(
    SnapshotKey           VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_SnapshotKey DEFAULT('CURRENT'),
    InventoryValueKpiId   VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_InventoryValueKpiId DEFAULT('PRN-INV-001'),
    InventoryDaysKpiId    VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_InventoryDaysKpiId DEFAULT('PRN-INV-002'),
    BusinessDate          DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_BusinessDate DEFAULT('3000-01-01'),
    GeneratedAt           DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId      VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalInventoryKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalInventoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalMomGrowth.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalMomGrowth
(
    PrincipalMomGrowthId   VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PrincipalMomGrowthId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SnapshotKey DEFAULT('CURRENT'),
    MomGrowthKpiId         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_MomGrowthKpiId DEFAULT('PRN-GRW-001'),
    SalesOutKpiId          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PeriodYear DEFAULT(0),
    PeriodMonth            INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PeriodMonth DEFAULT(0),
    PriorYear              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PriorYear DEFAULT(0),
    PriorMonth             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PriorMonth DEFAULT(0),
    SupplierId             VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SupplierId DEFAULT(''),
    SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SupplierName DEFAULT(''),
    CurrentSalesOutAmount  DECIMAL(18,2) NULL,
    PriorSalesOutAmount    DECIMAL(18,2) NULL,
    MomGrowthPercentage    DECIMAL(18,6) NULL,
    SortOrder              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SortOrder DEFAULT(0),
    GeneratedAt            DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId       VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalMomGrowth PRIMARY KEY CLUSTERED (PrincipalMomGrowthId),
    CONSTRAINT UX_BTRPD_PrincipalMomGrowth_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalMomGrowth_SnapshotKey_SortOrder
    ON BTRPD_PrincipalMomGrowth (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalMomGrowthKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalMomGrowthKpi
(
    SnapshotKey              VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_SnapshotKey DEFAULT('CURRENT'),
    MomGrowthKpiId           VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_MomGrowthKpiId DEFAULT('PRN-GRW-001'),
    SalesOutKpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PeriodYear DEFAULT(0),
    PeriodMonth              INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PeriodMonth DEFAULT(0),
    PriorYear                INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PriorYear DEFAULT(0),
    PriorMonth               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PriorMonth DEFAULT(0),
    GeneratedAt              DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalMomGrowthKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalPurchaseIn.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalPurchaseIn
(
    PrincipalPurchaseInId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_PrincipalPurchaseInId DEFAULT(''),
    SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_SnapshotKey DEFAULT('CURRENT'),
    KpiId                 VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_KpiId DEFAULT('PRN-PUR-001'),
    PeriodYear            INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_PeriodYear DEFAULT(0),
    PeriodMonth           INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_PeriodMonth DEFAULT(0),
    SupplierId            VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_SupplierId DEFAULT(''),
    SupplierName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_SupplierName DEFAULT(''),
    PurchaseInAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_PurchaseInAmount DEFAULT(0),
    LineCount             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_LineCount DEFAULT(0),
    SortOrder             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_SortOrder DEFAULT(0),
    GeneratedAt           DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId      VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalPurchaseIn PRIMARY KEY CLUSTERED (PrincipalPurchaseInId),
    CONSTRAINT UX_BTRPD_PrincipalPurchaseIn_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalPurchaseIn_SnapshotKey_SortOrder
    ON BTRPD_PrincipalPurchaseIn (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalPurchaseInKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalPurchaseInKpi
(
    SnapshotKey      VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_SnapshotKey DEFAULT('CURRENT'),
    KpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_KpiId DEFAULT('PRN-PUR-001'),
    PeriodYear       INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_PeriodYear DEFAULT(0),
    PeriodMonth      INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_PeriodMonth DEFAULT(0),
    GeneratedAt      DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalPurchaseInKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalReturn.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalReturn
(
    PrincipalReturnId   VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_PrincipalReturnId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_SnapshotKey DEFAULT('CURRENT'),
    GoodReturnKpiId     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_GoodReturnKpiId DEFAULT('PRN-RET-001'),
    BrokenReturnKpiId   VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_BrokenReturnKpiId DEFAULT('PRN-RET-002'),
    TotalReturnKpiId    VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    PeriodYear          INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_PeriodYear DEFAULT(0),
    PeriodMonth         INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_PeriodMonth DEFAULT(0),
    SupplierId          VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_SupplierId DEFAULT(''),
    SupplierName        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_SupplierName DEFAULT(''),
    GoodReturnAmount    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_GoodReturnAmount DEFAULT(0),
    BrokenReturnAmount  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_BrokenReturnAmount DEFAULT(0),
    TotalReturnAmount   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_TotalReturnAmount DEFAULT(0),
    LineCount           INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_LineCount DEFAULT(0),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_SortOrder DEFAULT(0),
    GeneratedAt         DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId    VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturn PRIMARY KEY CLUSTERED (PrincipalReturnId),
    CONSTRAINT UX_BTRPD_PrincipalReturn_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalReturn_SnapshotKey_SortOrder
    ON BTRPD_PrincipalReturn (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalReturnHistory.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalReturnHistory
(
    PrincipalReturnHistoryId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_PrincipalReturnHistoryId DEFAULT(''),
    GoodReturnKpiId          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_GoodReturnKpiId DEFAULT('PRN-RET-001'),
    BrokenReturnKpiId        VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_BrokenReturnKpiId DEFAULT('PRN-RET-002'),
    TotalReturnKpiId         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    PeriodYear               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_PeriodYear DEFAULT(0),
    PeriodMonth              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_PeriodMonth DEFAULT(0),
    SupplierId               VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_SupplierId DEFAULT(''),
    SupplierName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_SupplierName DEFAULT(''),
    GoodReturnAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_GoodReturnAmount DEFAULT(0),
    BrokenReturnAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_BrokenReturnAmount DEFAULT(0),
    TotalReturnAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_TotalReturnAmount DEFAULT(0),
    LineCount                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_LineCount DEFAULT(0),
    SortOrder                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_SortOrder DEFAULT(0),
    GeneratedAt              DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturnHistory PRIMARY KEY CLUSTERED (PrincipalReturnHistoryId),
    CONSTRAINT UX_BTRPD_PrincipalReturnHistory_Period_SupplierId UNIQUE (PeriodYear, PeriodMonth, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalReturnHistory_SupplierId_Period
    ON BTRPD_PrincipalReturnHistory (SupplierId, PeriodYear, PeriodMonth)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalReturnHistoryKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalReturnHistoryKpi
(
    SnapshotKey         VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_SnapshotKey DEFAULT('HISTORY'),
    GoodReturnKpiId     VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_GoodReturnKpiId DEFAULT('PRN-RET-001'),
    BrokenReturnKpiId   VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_BrokenReturnKpiId DEFAULT('PRN-RET-002'),
    TotalReturnKpiId    VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    GeneratedAt         DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId    VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturnHistoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalReturnKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalReturnKpi
(
    SnapshotKey        VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_SnapshotKey DEFAULT('CURRENT'),
    GoodReturnKpiId    VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_GoodReturnKpiId DEFAULT('PRN-RET-001'),
    BrokenReturnKpiId  VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_BrokenReturnKpiId DEFAULT('PRN-RET-002'),
    TotalReturnKpiId   VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    PeriodYear         INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_PeriodYear DEFAULT(0),
    PeriodMonth        INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_PeriodMonth DEFAULT(0),
    GeneratedAt        DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId   VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturnKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalReturnPercentage.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalReturnPercentage
(
    PrincipalReturnPercentageId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_PrincipalReturnPercentageId DEFAULT(''),
    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_SnapshotKey DEFAULT('CURRENT'),
    ReturnPercentageKpiId       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_ReturnPercentageKpiId DEFAULT('PRN-RET-004'),
    SalesOutKpiId               VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    TotalReturnKpiId            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    PeriodYear                  INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_PeriodYear DEFAULT(0),
    PeriodMonth                 INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_PeriodMonth DEFAULT(0),
    SupplierId                  VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_SupplierId DEFAULT(''),
    SupplierName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_SupplierName DEFAULT(''),
    TotalReturnAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_TotalReturnAmount DEFAULT(0),
    SalesOutAmount              DECIMAL(18,2) NULL,
    ReturnPercentage            DECIMAL(18,6) NULL,
    SortOrder                   INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_SortOrder DEFAULT(0),
    GeneratedAt                 DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId            VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturnPercentage PRIMARY KEY CLUSTERED (PrincipalReturnPercentageId),
    CONSTRAINT UX_BTRPD_PrincipalReturnPercentage_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalReturnPercentage_SnapshotKey_SortOrder
    ON BTRPD_PrincipalReturnPercentage (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalReturnPercentageKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalReturnPercentageKpi
(
    SnapshotKey             VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_SnapshotKey DEFAULT('CURRENT'),
    ReturnPercentageKpiId   VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_ReturnPercentageKpiId DEFAULT('PRN-RET-004'),
    SalesOutKpiId           VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    TotalReturnKpiId        VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    PeriodYear              INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_PeriodYear DEFAULT(0),
    PeriodMonth             INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_PeriodMonth DEFAULT(0),
    GeneratedAt             DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId        VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturnPercentageKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalSalesOut.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalSalesOut
(
    PrincipalSalesOutId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_PrincipalSalesOutId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_SnapshotKey DEFAULT('CURRENT'),
    KpiId               VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_KpiId DEFAULT('PRN-SALES-001'),
    PeriodYear          INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_PeriodYear DEFAULT(0),
    PeriodMonth         INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_PeriodMonth DEFAULT(0),
    SupplierId          VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_SupplierId DEFAULT(''),
    SupplierName        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_SupplierName DEFAULT(''),
    SalesOutAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_SalesOutAmount DEFAULT(0),
    LineCount           INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_LineCount DEFAULT(0),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_SortOrder DEFAULT(0),
    GeneratedAt         DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId    VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalSalesOut PRIMARY KEY CLUSTERED (PrincipalSalesOutId),
    CONSTRAINT UX_BTRPD_PrincipalSalesOut_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalSalesOut_SnapshotKey_SortOrder
    ON BTRPD_PrincipalSalesOut (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalSalesOutDataQuality.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalSalesOutDataQuality
(
    PrincipalSalesOutDataQualityId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_PrincipalSalesOutDataQualityId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_SnapshotKey DEFAULT('CURRENT'),
    PeriodYear                     INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_PeriodYear DEFAULT(0),
    PeriodMonth                    INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_PeriodMonth DEFAULT(0),
    ExceptionCode                  VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_ExceptionCode DEFAULT(''),
    Amount                         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_Amount DEFAULT(0),
    LineCount                      INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_LineCount DEFAULT(0),
    GeneratedAt                    DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId               VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalSalesOutDataQuality PRIMARY KEY CLUSTERED (PrincipalSalesOutDataQualityId),
    CONSTRAINT UX_BTRPD_PrincipalSalesOutDataQuality_SnapshotKey_ExceptionCode UNIQUE (SnapshotKey, ExceptionCode)
)
GO

CREATE INDEX IX_BTRPD_PrincipalSalesOutDataQuality_SnapshotKey_ExceptionCode
    ON BTRPD_PrincipalSalesOutDataQuality (SnapshotKey, ExceptionCode)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalSalesOutHistory.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalSalesOutHistory
(
    PrincipalSalesOutHistoryId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_PrincipalSalesOutHistoryId DEFAULT(''),
    KpiId                      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_KpiId DEFAULT('PRN-SALES-001'),
    PeriodYear                 INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_PeriodYear DEFAULT(0),
    PeriodMonth                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_PeriodMonth DEFAULT(0),
    SupplierId                 VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_SupplierId DEFAULT(''),
    SupplierName               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_SupplierName DEFAULT(''),
    SalesOutAmount             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_SalesOutAmount DEFAULT(0),
    LineCount                  INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_LineCount DEFAULT(0),
    SortOrder                  INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_SortOrder DEFAULT(0),
    GeneratedAt                DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId           VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalSalesOutHistory PRIMARY KEY CLUSTERED (PrincipalSalesOutHistoryId),
    CONSTRAINT UX_BTRPD_PrincipalSalesOutHistory_Period_SupplierId UNIQUE (PeriodYear, PeriodMonth, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalSalesOutHistory_SupplierId_Period
    ON BTRPD_PrincipalSalesOutHistory (SupplierId, PeriodYear, PeriodMonth)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalSalesOutHistoryKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalSalesOutHistoryKpi
(
    SnapshotKey           VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_SnapshotKey DEFAULT('HISTORY'),
    KpiId                 VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_KpiId DEFAULT('PRN-SALES-001'),
    HistoricalLimitation  VARCHAR(300) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_HistoricalLimitation DEFAULT(''),
    GeneratedAt           DATETIME     NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId      VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalSalesOutHistoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalSalesOutKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalSalesOutKpi
(
    SnapshotKey      VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_SnapshotKey DEFAULT('CURRENT'),
    KpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_KpiId DEFAULT('PRN-SALES-001'),
    PeriodYear       INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_PeriodYear DEFAULT(0),
    PeriodMonth      INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_PeriodMonth DEFAULT(0),
    GeneratedAt      DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalSalesOutKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalTarget.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalTarget
(
    PrincipalTargetId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_PrincipalTargetId DEFAULT(''),
    SnapshotKey       VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SnapshotKey DEFAULT('CURRENT'),
    KpiId             VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_KpiId DEFAULT('PRN-TGT-001'),
    PeriodYear        INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_PeriodYear DEFAULT(0),
    PeriodMonth       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_PeriodMonth DEFAULT(0),
    SupplierId        VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SupplierId DEFAULT(''),
    SupplierName      VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SupplierName DEFAULT(''),
    TargetAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_TargetAmount DEFAULT(0),
    SourceCount       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SourceCount DEFAULT(0),
    SortOrder         INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SortOrder DEFAULT(0),
    GeneratedAt       DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId  VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalTarget PRIMARY KEY CLUSTERED (PrincipalTargetId),
    CONSTRAINT UX_BTRPD_PrincipalTarget_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalTarget_SnapshotKey_SortOrder
    ON BTRPD_PrincipalTarget (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalTargetKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalTargetKpi
(
    SnapshotKey      VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_SnapshotKey DEFAULT('CURRENT'),
    KpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_KpiId DEFAULT('PRN-TGT-001'),
    PeriodYear       INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_PeriodYear DEFAULT(0),
    PeriodMonth      INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_PeriodMonth DEFAULT(0),
    GeneratedAt      DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalTargetKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalYoyGrowth.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalYoyGrowth
(
    PrincipalYoyGrowthId     VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_PrincipalYoyGrowthId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_SnapshotKey DEFAULT('CURRENT'),
    YoyGrowthKpiId           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_YoyGrowthKpiId DEFAULT('PRN-GRW-002'),
    SalesOutKpiId            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_PeriodYear DEFAULT(0),
    PeriodMonth              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_PeriodMonth DEFAULT(0),
    PriorYear                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_PriorYear DEFAULT(0),
    PriorMonth               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_PriorMonth DEFAULT(0),
    SupplierId               VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_SupplierId DEFAULT(''),
    SupplierName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_SupplierName DEFAULT(''),
    CurrentSalesOutAmount    DECIMAL(18,2) NULL,
    PriorSalesOutAmount      DECIMAL(18,2) NULL,
    YoyGrowthPercentage      DECIMAL(18,6) NULL,
    SortOrder                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_SortOrder DEFAULT(0),
    GeneratedAt              DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalYoyGrowth PRIMARY KEY CLUSTERED (PrincipalYoyGrowthId),
    CONSTRAINT UX_BTRPD_PrincipalYoyGrowth_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalYoyGrowth_SnapshotKey_SortOrder
    ON BTRPD_PrincipalYoyGrowth (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PrincipalYoyGrowthKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PrincipalYoyGrowthKpi
(
    SnapshotKey              VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_SnapshotKey DEFAULT('CURRENT'),
    YoyGrowthKpiId           VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_YoyGrowthKpiId DEFAULT('PRN-GRW-002'),
    SalesOutKpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PeriodYear DEFAULT(0),
    PeriodMonth              INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PeriodMonth DEFAULT(0),
    PriorYear                INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PriorYear DEFAULT(0),
    PriorMonth               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PriorMonth DEFAULT(0),
    GeneratedAt              DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalYoyGrowthKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PurchasingKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PurchasingKpi
(
    SnapshotKey                VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                 INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_PeriodYear DEFAULT(0),
    PeriodMonth                INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_PeriodMonth DEFAULT(0),
    GrandTotalPurchase         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_GrandTotalPurchase DEFAULT(0),
    TotalInvoice               INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_TotalInvoice DEFAULT(0),
    PendingPostingInvoiceCount INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_PendingPostingInvoiceCount DEFAULT(0),
    LastRefreshLogId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PurchasingKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PurchasingManagementAttention.sql
-- ============================================================

CREATE TABLE BTRPD_PurchasingManagementAttention
(
    PurchasingManagementAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_PurchasingManagementAttentionId DEFAULT(''),
    SnapshotKey                       VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SnapshotKey DEFAULT('CURRENT'),
    EntityType                        VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_EntityType DEFAULT(''),
    EntityName                        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_EntityName DEFAULT(''),
    SignalKey                         VARCHAR(40)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SignalKey DEFAULT(''),
    SignalLabel                       VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SignalLabel DEFAULT(''),
    ValueAmount                       DECIMAL(18,2) NULL,
    ValueText                         VARCHAR(100)  NULL,
    ReportRoute                       VARCHAR(100)  NULL,
    SortOrder                         INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingManagementAttention PRIMARY KEY CLUSTERED (PurchasingManagementAttentionId)
)
GO

CREATE INDEX IX_BTRPD_PurchasingManagementAttention_SnapshotKey_SortOrder
    ON BTRPD_PurchasingManagementAttention (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PurchasingManagementKpi.sql
-- ============================================================

CREATE TABLE BTRPD_PurchasingManagementKpi
(
    SnapshotKey                        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                         INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PeriodYear DEFAULT(0),
    PeriodMonth                        INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PeriodMonth DEFAULT(0),
    QualifiedBacklogCount              INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogCount DEFAULT(0),
    QualifiedBacklogValue              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogValue DEFAULT(0),
    PendingPostingValue                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PendingPostingValue DEFAULT(0),
    PostedPercent                      DECIMAL(9,4)  NULL,
    Top1PrincipalPercent               DECIMAL(9,4)  NULL,
    Top3PrincipalPercent               DECIMAL(9,4)  NULL,
    Top1SupplierInventoryPercent       DECIMAL(9,4)  NULL,
    CompoundDependencyCount            INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_CompoundDependencyCount DEFAULT(0),
    PrincipalInventoryNoPurchaseCount  INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PrincipalInventoryNoPurchaseCount DEFAULT(0),
    UnknownPrincipalCount              INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_UnknownPrincipalCount DEFAULT(0),
    PurchasingInactivityFlag           BIT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PurchasingInactivityFlag DEFAULT(0),
    QualifiedBacklogPrincipalCount     INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogPrincipalCount DEFAULT(0),
    PrincipalAtRiskExposureCount       INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PrincipalAtRiskExposureCount DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PurchasingManagementKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PurchasingManagementTopPrincipal.sql
-- ============================================================

CREATE TABLE BTRPD_PurchasingManagementTopPrincipal
(
    PurchasingManagementTopPrincipalId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_PurchasingManagementTopPrincipalId DEFAULT(''),
    SnapshotKey                          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey DEFAULT('CURRENT'),
    Rank                                 INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_Rank DEFAULT(0),
    PrincipalName                        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_PrincipalName DEFAULT(''),
    MtdPurchaseAmount                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_MtdPurchaseAmount DEFAULT(0),
    PercentOfPurchase                    DECIMAL(9,4)  NULL,
    InventoryValue                       DECIMAL(18,2) NULL,
    PercentOfInventory                   DECIMAL(9,4)  NULL,
    AtRiskValue                          DECIMAL(18,2) NULL,
    PercentOfAtRisk                      DECIMAL(9,4)  NULL,
    IsCompoundDependency                 BIT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_IsCompoundDependency DEFAULT(0),
    IsInventoryNoPurchase                BIT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_IsInventoryNoPurchase DEFAULT(0),
    ReportRoute                          VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_ReportRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_PurchasingManagementTopPrincipal PRIMARY KEY CLUSTERED (PurchasingManagementTopPrincipalId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey_Rank
    ON BTRPD_PurchasingManagementTopPrincipal (SnapshotKey, Rank)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PurchasingPostingStatus.sql
-- ============================================================

CREATE TABLE BTRPD_PurchasingPostingStatus
(
    PurchasingPostingStatusId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_PurchasingPostingStatusId DEFAULT(''),
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_SnapshotKey DEFAULT('CURRENT'),
    StatusKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_StatusKey DEFAULT(''),
    StatusLabel               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_StatusLabel DEFAULT(''),
    SortOrder                 INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_SortOrder DEFAULT(0),
    PurchaseAmount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingPostingStatus PRIMARY KEY CLUSTERED (PurchasingPostingStatusId),
    CONSTRAINT UX_BTRPD_PurchasingPostingStatus_SnapshotKey_StatusKey UNIQUE (SnapshotKey, StatusKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PurchasingTopPrincipal.sql
-- ============================================================

CREATE TABLE BTRPD_PurchasingTopPrincipal
(
    PurchasingTopPrincipalId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_PurchasingTopPrincipalId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_Rank DEFAULT(0),
    PrincipalName            VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_PrincipalName DEFAULT(''),
    PurchaseAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingTopPrincipal PRIMARY KEY CLUSTERED (PurchasingTopPrincipalId),
    CONSTRAINT UX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO

CREATE INDEX IX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank
    ON BTRPD_PurchasingTopPrincipal (SnapshotKey, Rank)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_PurchasingWeekTrend.sql
-- ============================================================

CREATE TABLE BTRPD_PurchasingWeekTrend
(
    PurchasingWeekTrendId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_PurchasingWeekTrendId DEFAULT(''),
    SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_SnapshotKey DEFAULT('CURRENT'),
    WeekStart             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_WeekStart DEFAULT('3000-01-01'),
    WeekEnd               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_WeekEnd DEFAULT('3000-01-01'),
    WeekLabel             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_WeekLabel DEFAULT(''),
    PurchaseAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingWeekTrend PRIMARY KEY CLUSTERED (PurchasingWeekTrendId)
)
GO

CREATE INDEX IX_BTRPD_PurchasingWeekTrend_SnapshotKey_WeekStart
    ON BTRPD_PurchasingWeekTrend (SnapshotKey, WeekStart)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_RefreshLog.sql
-- ============================================================

CREATE TABLE BTRPD_RefreshLog
(
    RefreshLogId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_RefreshLogId DEFAULT(''),
    Domain         VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_Domain DEFAULT(''),
    StartedAt      DATETIME     NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_StartedAt DEFAULT('3000-01-01'),
    CompletedAt    DATETIME     NULL,
    Status         VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_Status DEFAULT(''),
    DurationMs     INT          NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_DurationMs DEFAULT(0),
    ErrorMessage   VARCHAR(500) NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_ErrorMessage DEFAULT(''),
    TriggeredBy    VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_TriggeredBy DEFAULT(''),

    CONSTRAINT PK_BTRPD_RefreshLog PRIMARY KEY CLUSTERED (RefreshLogId)
)
GO

CREATE INDEX IX_BTRPD_RefreshLog_Domain_CompletedAt
    ON BTRPD_RefreshLog (Domain, CompletedAt DESC)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesDailyPace.sql
-- ============================================================

CREATE TABLE BTRPD_SalesDailyPace
(
    SalesDailyPaceId     VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_SalesDailyPaceId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_SnapshotKey DEFAULT('CURRENT'),
    PaceDate             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_PaceDate DEFAULT('3000-01-01'),
    DayOfMonth           INT           NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_DayOfMonth DEFAULT(0),
    IsElapsed            BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_IsElapsed DEFAULT(0),
    ActualAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_ActualAmount DEFAULT(0),
    ProjectedDailyAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_ProjectedDailyAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesDailyPace PRIMARY KEY CLUSTERED (SalesDailyPaceId)
)
GO

CREATE INDEX IX_BTRPD_SalesDailyPace_SnapshotKey_PaceDate
    ON BTRPD_SalesDailyPace (SnapshotKey, PaceDate)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesForecastKpi.sql
-- ============================================================

CREATE TABLE BTRPD_SalesForecastKpi
(
    SnapshotKey                 VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                 DATETIME       NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                  INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_PeriodYear DEFAULT(0),
    PeriodMonth                 INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_PeriodMonth DEFAULT(0),
    BusinessDate                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    DaysInMonth                 INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DaysInMonth DEFAULT(0),
    DaysElapsed                 INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DaysElapsed DEFAULT(0),
    DaysRemaining               INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DaysRemaining DEFAULT(0),
    CurrentSales                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_CurrentSales DEFAULT(0),
    TotalTarget                 DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_TotalTarget DEFAULT(0),
    CurrentAchievementPercent   DECIMAL(9,4)   NULL,
    DailyAverageSales           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DailyAverageSales DEFAULT(0),
    ForecastSales               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastSales DEFAULT(0),
    ForecastAchievementPercent  DECIMAL(9,4)   NULL,
    RequiredDailySales          DECIMAL(18,2)  NULL,
    TargetGap                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_TargetGap DEFAULT(0),
    ForecastVariance            DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastVariance DEFAULT(0),
    BestCaseSales               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_BestCaseSales DEFAULT(0),
    WorstCaseSales              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_WorstCaseSales DEFAULT(0),
    ForecastConfidence          VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastConfidence DEFAULT(''),
    ForecastRiskBand              VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastRiskBand DEFAULT(''),
    LastRefreshLogId            VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_SalesForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesKpi.sql
-- ============================================================

CREATE TABLE BTRPD_SalesKpi
(
    SnapshotKey        VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt        DATETIME       NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear         INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_PeriodYear DEFAULT(0),
    PeriodMonth        INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_PeriodMonth DEFAULT(0),
    TotalOmzet         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalOmzet DEFAULT(0),
    TotalFaktur        INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalFaktur DEFAULT(0),
    TotalCustomer      INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalCustomer DEFAULT(0),
    TotalTarget        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalTarget DEFAULT(0),
    TotalAchievement   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalAchievement DEFAULT(0),
    AchievementPercent DECIMAL(9,4)   NULL,
    CompletedOmzet     DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_CompletedOmzet DEFAULT(0),
    PipelineOmzet      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_PipelineOmzet DEFAULT(0),
    LastRefreshLogId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_SalesKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesmanAttention.sql
-- ============================================================

CREATE TABLE BTRPD_SalesmanAttention
(
    SalesmanAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SalesmanAttentionId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SnapshotKey DEFAULT('CURRENT'),
    SalesPersonId       VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SalesPersonId DEFAULT(''),
    SalesPersonCode     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SalesPersonCode DEFAULT(''),
    SalesPersonName     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SalesPersonName DEFAULT(''),
    SignalKey           VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SignalKey DEFAULT(''),
    SignalLabel         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SignalLabel DEFAULT(''),
    ValueAmount         DECIMAL(18,2) NULL,
    ValueText           VARCHAR(100)  NULL,
    WilayahName         VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_WilayahName DEFAULT(''),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_SortOrder DEFAULT(0),
    IsActive            BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanAttention_IsActive DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanAttention PRIMARY KEY CLUSTERED (SalesmanAttentionId)
)
GO

CREATE INDEX IX_BTRPD_SalesmanAttention_SnapshotKey_SortOrder
    ON BTRPD_SalesmanAttention (SnapshotKey, SortOrder)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesmanKpi.sql
-- ============================================================

CREATE TABLE BTRPD_SalesmanKpi
(
    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                 DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                  INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_PeriodYear DEFAULT(0),
    PeriodMonth                 INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_PeriodMonth DEFAULT(0),
    TotalTeamOmzet              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_TotalTeamOmzet DEFAULT(0),
    TotalPiutang                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_TotalPiutang DEFAULT(0),
    ActiveSalesmanCount         INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_ActiveSalesmanCount DEFAULT(0),
    BelowTargetCount            INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_BelowTargetCount DEFAULT(0),
    MissingTargetSetupCount     INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_MissingTargetSetupCount DEFAULT(0),
    HighOverdueExposureCount    INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_HighOverdueExposureCount DEFAULT(0),
    HighPiutangExposureCount    INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_HighPiutangExposureCount DEFAULT(0),
    CustomerConcentrationCount  INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_CustomerConcentrationCount DEFAULT(0),
    DormantPortfolioCount       INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_DormantPortfolioCount DEFAULT(0),
    TopOmzetSalesmanPercent     DECIMAL(9,4)  NULL,
    TopPiutangSalesmanPercent   DECIMAL(9,4)  NULL,
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_SalesmanKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesmanPrincipalAchievement.sql
-- ============================================================

CREATE TABLE BTRPD_SalesmanPrincipalAchievement
(
    SalesmanPrincipalAchievementId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SalesmanPrincipalAchievementId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SnapshotKey DEFAULT('CURRENT'),
    SalesPersonId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SalesPersonId DEFAULT(''),
    SalesPersonCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SalesPersonCode DEFAULT(''),
    SalesPersonName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SalesPersonName DEFAULT(''),
    SupplierId                     VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SupplierId DEFAULT(''),
    SupplierName                   VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SupplierName DEFAULT(''),
    TargetAmount                   DECIMAL(18,2) NULL,
    CompletedOmzet                 DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_CompletedOmzet DEFAULT(0),
    AchievementPercent             DECIMAL(9,4)  NULL,
    SortOrder                      INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanPrincipalAchievement PRIMARY KEY CLUSTERED (SalesmanPrincipalAchievementId),
    CONSTRAINT UX_BTRPD_SalesmanPrincipalAchievement_SnapshotKey_SalesPersonId_SupplierId UNIQUE (SnapshotKey, SalesPersonId, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_SalesmanPrincipalAchievement_SnapshotKey_SalesPersonId
    ON BTRPD_SalesmanPrincipalAchievement (SnapshotKey, SalesPersonId)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesmanRepHistory.sql
-- ============================================================

CREATE TABLE BTRPD_SalesmanRepHistory
(
    SalesmanRepHistoryId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesmanRepHistoryId DEFAULT(''),
    PeriodYear           INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_PeriodYear DEFAULT(0),
    PeriodMonth          INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_PeriodMonth DEFAULT(0),
    SalesPersonId        VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesPersonId DEFAULT(''),
    SalesPersonCode      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesPersonCode DEFAULT(''),
    SalesPersonName      VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesPersonName DEFAULT(''),
    TargetAmount         DECIMAL(18,2) NULL,
    CompletedOmzet       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_CompletedOmzet DEFAULT(0),
    AchievementPercent   DECIMAL(9,4)  NULL,
    AchievementBand      VARCHAR(20)   NULL,
    OpenBalance          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_OpenBalance DEFAULT(0),
    IsActive             BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_IsActive DEFAULT(0),
    LastRefreshLogId     VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_LastRefreshLogId DEFAULT(''),
    UpdatedAt            DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_UpdatedAt DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTRPD_SalesmanRepHistory PRIMARY KEY CLUSTERED (SalesmanRepHistoryId),
    CONSTRAINT UX_BTRPD_SalesmanRepHistory_PeriodYear_PeriodMonth_SalesPersonId UNIQUE (PeriodYear, PeriodMonth, SalesPersonId)
)
GO

CREATE INDEX IX_BTRPD_SalesmanRepHistory_SalesPersonId
    ON BTRPD_SalesmanRepHistory (SalesPersonId, PeriodYear DESC, PeriodMonth DESC)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesmanSegmentation.sql
-- ============================================================

CREATE TABLE BTRPD_SalesmanSegmentation
(
    SalesmanSegmentationId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SalesmanSegmentationId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SnapshotKey DEFAULT('CURRENT'),
    SegmentType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SegmentType DEFAULT(''),
    SegmentKey             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SegmentKey DEFAULT(''),
    SegmentLabel           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SegmentLabel DEFAULT(''),
    SalesmanCount          INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SalesmanCount DEFAULT(0),
    ActiveCount            INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_ActiveCount DEFAULT(0),
    InactiveCount          INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_InactiveCount DEFAULT(0),
    SortOrder              INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanSegmentation PRIMARY KEY CLUSTERED (SalesmanSegmentationId),
    CONSTRAINT UX_BTRPD_SalesmanSegmentation_SnapshotKey_SegmentType_SegmentKey UNIQUE (SnapshotKey, SegmentType, SegmentKey)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesmanTopAchievement.sql
-- ============================================================

CREATE TABLE BTRPD_SalesmanTopAchievement
(
    SalesmanTopAchievementId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesmanTopAchievementId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_Rank DEFAULT(0),
    SalesPersonId            VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesPersonId DEFAULT(''),
    SalesPersonCode          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesPersonCode DEFAULT(''),
    SalesPersonName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesPersonName DEFAULT(''),
    TargetAmount             DECIMAL(18,2) NULL,
    CompletedOmzet           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_CompletedOmzet DEFAULT(0),
    AchievementPercent       DECIMAL(9,4)  NULL,
    PercentOfTotal           DECIMAL(9,4)  NULL,
    IsActive                 BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_IsActive DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanTopAchievement PRIMARY KEY CLUSTERED (SalesmanTopAchievementId),
    CONSTRAINT UX_BTRPD_SalesmanTopAchievement_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesmanTopOmzet.sql
-- ============================================================

CREATE TABLE BTRPD_SalesmanTopOmzet
(
    SalesmanTopOmzetId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesmanTopOmzetId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_Rank DEFAULT(0),
    SalesPersonId      VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesPersonId DEFAULT(''),
    SalesPersonCode    VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesPersonCode DEFAULT(''),
    SalesPersonName    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesPersonName DEFAULT(''),
    CompletedOmzet     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_CompletedOmzet DEFAULT(0),
    PercentOfTotal     DECIMAL(9,4)  NULL,
    IsActive           BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_IsActive DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanTopOmzet PRIMARY KEY CLUSTERED (SalesmanTopOmzetId),
    CONSTRAINT UX_BTRPD_SalesmanTopOmzet_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesmanTopPiutang.sql
-- ============================================================

CREATE TABLE BTRPD_SalesmanTopPiutang
(
    SalesmanTopPiutangId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesmanTopPiutangId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_Rank DEFAULT(0),
    SalesPersonId        VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesPersonId DEFAULT(''),
    SalesPersonCode      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesPersonCode DEFAULT(''),
    SalesPersonName      VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesPersonName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_OutstandingBalance DEFAULT(0),
    PercentOfTotal       DECIMAL(9,4)  NULL,
    IsActive             BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_IsActive DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanTopPiutang PRIMARY KEY CLUSTERED (SalesmanTopPiutangId),
    CONSTRAINT UX_BTRPD_SalesmanTopPiutang_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesTopSalesman.sql
-- ============================================================

CREATE TABLE BTRPD_SalesTopSalesman
(
    SalesTopSalesmanId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_SalesTopSalesmanId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_Rank DEFAULT(0),
    SalesPersonName    VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_SalesPersonName DEFAULT(''),
    CompletedOmzet     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_CompletedOmzet DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesTopSalesman PRIMARY KEY CLUSTERED (SalesTopSalesmanId),
    CONSTRAINT UX_BTRPD_SalesTopSalesman_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO


-- ============================================================
-- File: \ReportingContext\BTRPD_SalesWeekTrend.sql
-- ============================================================

CREATE TABLE BTRPD_SalesWeekTrend
(
    SalesWeekTrendId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_SalesWeekTrendId DEFAULT(''),
    SnapshotKey      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_SnapshotKey DEFAULT('CURRENT'),
    WeekStart        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_WeekStart DEFAULT('3000-01-01'),
    WeekEnd          DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_WeekEnd DEFAULT('3000-01-01'),
    WeekLabel        VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_WeekLabel DEFAULT(''),
    RecognizedAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_RecognizedAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesWeekTrend PRIMARY KEY CLUSTERED (SalesWeekTrendId)
)
GO

CREATE INDEX IX_BTRPD_SalesWeekTrend_SnapshotKey_WeekStart
    ON BTRPD_SalesWeekTrend (SnapshotKey, WeekStart)
GO


-- ============================================================
-- File: \ReportingContext\IX_BTR_Piutang_OpenBalance.sql
-- ============================================================

CREATE INDEX IX_BTR_Piutang_OpenBalance
    ON [dbo].[BTR_Piutang] (Sisa, PiutangId)
    INCLUDE (DueDate, Total, CustomerId)
    WHERE Sisa > 1
GO


-- ============================================================
-- File: \SalesContext\BTR_AlokasiFp.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_AlokasiFp]
(
	AlokasiFpId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_AlokasiFp_AlokasiFpId DEFAULT(''),
	AlokasiFpDate DATETIME NOT NULL CONSTRAINT DF_BTR_AlokasiFp_AlokasiFpDate DEFAULT(''),
	NoAwal VARCHAR(19) NOT NULL CONSTRAINT DF_BTR_AlokasiFp_NoAwal DEFAULT(''),
	NoAkhir VARCHAR(19) NOT NULL CONSTRAINT DF_BTR_AlokasiFp_NoAkhir DEFAULT(''),
	Kapasitas INT NOT NULL CONSTRAINT DF_BTR_AlokasiFp_Kapasitas DEFAULT(0),
	Sisa INT NOT NULL CONSTRAINT DF_BTR_AlokasiFp_Sisa DEFAULT(0),

	CONSTRAINT PK_BTR_AlokasiFp PRIMARY KEY CLUSTERED(AlokasiFpId)
)


-- ============================================================
-- File: \SalesContext\BTR_AlokasiFpItem.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_AlokasiFpItem]
(
	AlokasiFpId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_AlokasiFpItem_AlokasiFpId DEFAULT(''),
	NoFakturPajak VARCHAR(19) NOT NULL CONSTRAINT DF_BTR_AlokasiFpItem_NoFakturPajak DEFAULT(''),
	NoUrut INT NOT NULL CONSTRAINT DF_BTR_AlokasiFpItem_NoUrut DEFAULT(0),
	FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_AlokasiFpItem_FakturId DEFAULT(''),
	FakturCode VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_AlokasiFpItem_FakturCode DEFAULT(''),

	CONSTRAINT PK_BTR_AlokasiFpItem PRIMARY KEY CLUSTERED (AlokasiFpId, NoUrut)
)
GO

CREATE UNIQUE INDEX UX_BTR_AlokasiFpItem_FakturId 
	ON BTR_AlokasiFpItem (FakturId)
	WHERE FakturId <> ''
GO

CREATE UNIQUE INDEX UX_BTR_AlokasiFpItem_NoFakturPajak
	ON BTR_AlokasiFpItem (NoFakturPajak)
GO



-- ============================================================
-- File: \SalesContext\BTR_CheckIn.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_CheckIn]
(
    CheckInId VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_CheckIn_CheckInId DEFAULT(''),
    CheckInDate VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_CheckIn_CheckInDate DEFAULT('3000-01-01'),
    CheckInTime VARCHAR(8) NOT NULL CONSTRAINT DF_BTR_CheckIn_CheckInTime DEFAULT('00:00:00'),
    UserEmail VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_CheckIn_UserEmail DEFAULT(''),
    CheckInLatitude FLOAT NOT NULL CONSTRAINT DF_BTR_CheckIn_CheckInLatitude DEFAULT(0),
    CheckInLongitude FLOAT NOT NULL CONSTRAINT DF_BTR_CheckIn_CheckInLongitude DEFAULT(0),
    Accuracy FLOAT NOT NULL CONSTRAINT DF_BTR_CheckIn_Accuracy DEFAULT(0),
    CustomerId VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_CheckIn_CustomerId DEFAULT(''),
    CustomerCode VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_CheckIn_CustomerCode DEFAULT(''),
    CustomerName VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_CheckIn_CustomerName DEFAULT(''),
    CustomerAddress VARCHAR(200) NOT NULL CONSTRAINT DF_BTR_CheckIn_CustomerAddress DEFAULT(''),
    CustomerLatitude FLOAT NOT NULL CONSTRAINT DF_BTR_CheckIn_CustomerLatitude DEFAULT(0),
    CustomerLongitude FLOAT NOT NULL CONSTRAINT DF_BTR_CheckIn_CustomerLongitude DEFAULT(0),
    StatusSync VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_CheckIn_StatusSync DEFAULT(''),
    CheckOutTime VARCHAR(8) NOT NULL CONSTRAINT DF_BTR_CheckIn_CheckOutTime DEFAULT(''),
    CheckOutLatitude FLOAT NOT NULL CONSTRAINT DF_BTR_CheckIn_CheckOutLatitude DEFAULT(0),
    CheckOutLongitude FLOAT NOT NULL CONSTRAINT DF_BTR_CheckIn_CheckOutLongitude DEFAULT(0),
    CheckOutAccuracy FLOAT NOT NULL CONSTRAINT DF_BTR_CheckIn_CheckOutAccuracy DEFAULT(0),
    CheckOutMode VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_CheckIn_CheckOutMode DEFAULT(''),

    CONSTRAINT PK_BTR_CheckIn PRIMARY KEY CLUSTERED (CheckInId ASC)
)

GO

-- Optional: Create indexes for better query performance
--CREATE NONCLUSTERED INDEX IX_BTR_CheckIn_UserEmail 
--ON [dbo].[BTR_CheckIn] (UserEmail)

--CREATE NONCLUSTERED INDEX IX_BTR_CheckIn_CheckInDate 
--ON [dbo].[BTR_CheckIn] (CheckInDate)

--CREATE NONCLUSTERED INDEX IX_BTR_CheckIn_StatusSync 
--ON [dbo].[BTR_CheckIn] (StatusSync)

--CREATE NONCLUSTERED INDEX IX_BTR_CheckIn_CustomerId 
--ON [dbo].[BTR_CheckIn] (CustomerId)

--GO

-- ============================================================
-- File: \SalesContext\BTR_Customer.sql
-- ============================================================

CREATE TABLE BTR_Customer(
    CustomerId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_Customer_CustomerId DEFAULT(''),
    CustomerName VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Customer_CustomerName DEFAULT(''),
    CustomerCode VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_Customer_CustomerCode DEFAULT(''),

    WilayahId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_Customer_WilayahId DEFAULT(''),
    KlasifikasiId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Customer_KlasifikasiId DEFAULT(''),
    HargaTypeId VARCHAR(2) NOT NULL CONSTRAINT DF_BTR_Customer_HargaTypeId DEFAULT(''),

    Address1 VARCHAR(60) NOT NULL CONSTRAINT DF_BTR_Customer_Address1 DEFAULT(''),
    Address2 VARCHAR(60) NOT NULL CONSTRAINT DF_BTR_Customer_Address2 DEFAULT(''),
    Kota VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Customer_Kota DEFAULT(''),
    KodePos VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_Customer_KodePos DEFAULT(''),
    NoTelp VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Customer_NoTelp DEFAULT(''),
    NoFax VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Customer_NoFax DEFAULT(''),
    
    Email VARCHAR(100) NOT NULL CONSTRAINT DF_BTR_Customer_Email DEFAULT(''),
    Nitku VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Customer_Nitku DEFAULT(''),

    Npwp VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Customer_Npwp DEFAULT(''),
    Nppkp VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Customer_Nppkp DEFAULT(''),
    Nik VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Customer_Nik DEFAULT(''),
    NamaWp VARCHAR(60) NOT NULL CONSTRAINT DF_BTR_Customer_NamaWp DEFAULT(''),
    AddressWp VARCHAR(128) NOT NULL CONSTRAINT DF_BTR_Customer_AlamatWp DEFAULT(''),
    AddressWp2 VARCHAR(60) NOT NULL CONSTRAINT DF_BTR_Customer_AlamatWp2 DEFAULT(''),
    IsKenaPajak BIT NOT NULL CONSTRAINT DF_BTR_Customer_IsKenaPajak DEFAULT(0),
    JenisIdentitasPajak VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Customer_JenisIdentitasPajak DEFAULT(''),
    
    IsSuspend BIT NOT NULL CONSTRAINT DF_BTR_Customer_IsSuspend DEFAULT(0),
    Plafond DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Customer_Plafond DEFAULT(0),
    CreditBalance DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Customer_CreditBalance DEFAULT(0),

    Latitude FLOAT NOT NULL CONSTRAINT DF_BTR_Customer_Latitude DEFAULT(0),
    Longitude FLOAT NOT NULL CONSTRAINT DF_BTR_Customer_Longitude DEFAULT(0),
    Accuracy FLOAT NOT NULL CONSTRAINT DF_BTR_Customer_Accuracy DEFAULT(0),
    CoordinateTimestamp FLOAT NOT NULL CONSTRAINT DF_BTR_Customer_CoordinateTimestamp DEFAULT(0),
    CoordinateUser FLOAT NOT NULL CONSTRAINT DF_BTR_Customer_CoordinateUser DEFAULT(0)

    CONSTRAINT  PK_BTR_Customer PRIMARY KEY CLUSTERED(CustomerId)
)

-- ============================================================
-- File: \SalesContext\BTR_CustomerLocHistory.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_CustomerLocHist]
(
	LocHistId VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_CustomerLocHist_LocHistId DEFAULT(''),
	CustomerId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_CustomerLocHist_CustomerId DEFAULT(''),
	ChangeDate DATETIME NOT NULL CONSTRAINT DF_BTR_CustomerLocHist_ChangeDate DEFAULT('3000-01-01'),
	Latitude FLOAT NOT NULL CONSTRAINT DF_BTR_CustomerLocHist_Latitude DEFAULT(0),
	Longitude FLOAT NOT NULL CONSTRAINT DF_BTR_CustomerLocHist_Longitude DEFAULT(0),
	Accuracy FLOAT NOT NULL CONSTRAINT DF_BTR_CustomerLocHist_Accuracy DEFAULT(0),
	ChangeUser VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_CustomerLocHist_ChangeUser DEFAULT(''),

	CONSTRAINT PK_BTR_CustomerLocHist PRIMARY KEY CLUSTERED (LocHistId)
)
GO

CREATE INDEX IX_BTR_CustomerLocHist_CustomerId
	ON BTR_CustomerLocHist(CustomerId, ChangeDate, LocHistId)
	WITH(FILLFACTOR=80)
GO


-- ============================================================
-- File: \SalesContext\BTR_Faktur.sql
-- ============================================================

CREATE TABLE BTR_Faktur
(
    FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Faktur_FakturId DEFAULT(''),
    FakturDate DATETIME NOT NULL CONSTRAINT DF_BTR_Faktur_FakturDate DEFAULT('3000-01-01'),
    FakturCode VARCHAR(11) NOT NULL CONSTRAINT DF_BTR_Faktur_FakturCode DEFAULT(''),
    FakturCodeOri VARCHAR(8) NOT NULL CONSTRAINT DF_BTR_Faktur_FakturCodeOri DEFAULT(''),

    SalesPersonId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Faktur_SalesPersonId DEFAULT(''),
    CustomerId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_Faktur_CustomerId DEFAULT(''),
    HargaTypeId VARCHAR(2) NOT NULL CONSTRAINT DF_BTR_Faktur_HargaTypeId DEFAULT(''),
    WarehouseId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Faktur_WarehouseId DEFAULT(''),
    TglRencanaKirim DATETIME NOT NULL CONSTRAINT DF_BTR_Faktur_TglRencanaKirim DEFAULT('3000-01-01'),
    DriverId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Faktur_DriverId DEFAULT(''),
    TermOfPayment INT NOT NULL CONSTRAINT DF_BTR_Faktur_TermOfPayment DEFAULT(0),
    DueDate DATETIME NOT NULL CONSTRAINT DF_BTR_Faktur_DueDate DEFAULT('3000-01-01'),

    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_Total DEFAULT(0),
    Discount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_Discount DEFAULT(0),
    Dpp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_Dpp DEFAULT(0),
    Tax DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_Tax DEFAULT(0),
    GrandTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_GrandTotal DEFAULT(0),

    TotalKlaim DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_TotalKlaim DEFAULT(0),
    DiscountKlaim DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_DiscountKlaim DEFAULT(0),
    DppKlaim DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_DppKlaim DEFAULT(0),
    TaxKlaim DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_TaxKlaim DEFAULT(0),
    GrandTotalKlaim DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_GrandTotalKlaim DEFAULT(0),

    UangMuka DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_UangMuka DEFAULT(0),
    KurangBayar DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_Faktur_KurangBayar DEFAULT(0),
    NoFakturPajak VARCHAR(19) NOT NULL CONSTRAINT DF_BTR_Faktur_NoFakturPajak DEFAULT(''),
    FpKeluaranId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_Faktur_FpKeluaranId DEFAULT(''),

    CreateTime DATETIME NOT NULL CONSTRAINT DF_BTR_Faktur_CreateTime DEFAULT('3000-01-01'),
    LastUpdate DATETIME NOT NULL CONSTRAINT DF_BTR_Faktur_LastUpdate DEFAULT('3000-01-01'),
    UserId VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Faktur_UserId DEFAULT(''),

    VoidDate DATETIME NOT NULL CONSTRAINT DF_BTR_Faktur_VoidDate DEFAULT('3000-01-01'),
    UserIdVoid VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Faktur_UserIdVoid DEFAULT(''),
    VoidReasonCode INT NOT NULL CONSTRAINT DF_BTR_Faktur_VoidReasonCode DEFAULT(0),
    VoidReasonNote VARCHAR(200) NOT NULL CONSTRAINT DF_BTR_Faktur_VoidReasonNote DEFAULT(''),

    Note VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Faktur_Note DEFAULT(''),
    IsHasKlaim BIT NOT NULL CONSTRAINT DF_BTR_Faktur_IsHasKlaim DEFAULT(0),
    OrderId VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_Faktur_OrderId DEFAULT(''),

    CONSTRAINT PK_BTR_Faktur PRIMARY KEY CLUSTERED (FakturId)
)
GO

CREATE INDEX IX_BTR_Faktur_FakturDate
    ON BTR_Faktur(FakturDate, FakturId)
GO

CREATE UNIQUE INDEX IX_BTR_Faktur_FakturCode
    ON BTR_Faktur (FakturCode)
    WHERE FakturCode <> ''
GO

CREATE UNIQUE INDEX IX_BTR_Faktur_OrderId
    ON BTR_Faktur (OrderId) 
    WHERE OrderId <> ''
GO




-- ============================================================
-- File: \SalesContext\BTR_FakturControlStatus.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_FakturControlStatus]
(
	FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FakturControlStatus_FakturId DEFAULT(''),
	FakturDate DATETIME NOT NULL CONSTRAINT DF_BTR_FakturControlStatus_FakturDate DEFAULT('3000-01-01'),
	StatusFaktur INT NOT NULL CONSTRAINT DF_BTR_FakturControlStatus_FakturStatus DEFAULT(0),
	StatusDate DATETIME NOT NULL CONSTRAINT DF_BTR_FakturControlStatus_StatusDate DEFAULT('3000-01-01'),
	Keterangan VARCHAR(255) NOT NULL CONSTRAINT DF_BTR_FakturControlStatus_Keterangan DEFAULT(''),
	UserId VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FakturControlStatus_UserId DEFAULT(''),

	CONSTRAINT PK_BTR_FakturControlStatus PRIMARY KEY CLUSTERED(FakturId, StatusFaktur)
)


-- ============================================================
-- File: \SalesContext\BTR_FakturDiscount.sql
-- ============================================================

CREATE TABLE BTR_FakturDiscount
(
    FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FakturDiscount_FakturId DEFAULT(''),
    FakturItemId VARCHAR(16) NOT NULL CONSTRAINT DF_BTR_FakturDiscount_FakturItemId DEFAULT(''),
    FakturDiscountId VARCHAR(18) NOT NULL CONSTRAINT DF_BTR_FakturDiscount_FakturDiscountId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_FakturDiscount_NoUrut DEFAULT(0),
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_FakturDiscount_BrgId DEFAULT(''),
    DiscProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturDiscount_DiscountProsen DEFAULT(0),
    DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturDiscount_DiscountRp DEFAULT(0),
    
    CONSTRAINT PK_BTR_FakturDiscount PRIMARY KEY CLUSTERED (FakturDiscountId)
)
GO

CREATE INDEX IX_BTR_FakturDiscount_FakturId
    ON BTR_FakturDiscount (FakturId, FakturDiscountId)
GO

-- ============================================================
-- File: \SalesContext\BTR_FakturDiscountKlaim.sql
-- ============================================================

CREATE TABLE BTR_FakturDiscountKlaim
(
    FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FakturDiscountKlaim_FakturId DEFAULT(''),
    FakturItemId VARCHAR(16) NOT NULL CONSTRAINT DF_BTR_FakturDiscountKlaim_FakturItemId DEFAULT(''),
    FakturDiscountId VARCHAR(18) NOT NULL CONSTRAINT DF_BTR_FakturDiscountKlaim_FakturDiscountId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_FakturDiscountKlaim_NoUrut DEFAULT(0),
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_FakturDiscountKlaim_BrgId DEFAULT(''),
    DiscProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturDiscountKlaim_DiscountProsen DEFAULT(0),
    DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturDiscountKlaim_DiscountRp DEFAULT(0),
    
    CONSTRAINT PK_BTR_FakturDiscountKlaim PRIMARY KEY CLUSTERED (FakturDiscountId)
)
GO

CREATE INDEX IX_BTR_FakturDiscountKlaim_FakturId
    ON BTR_FakturDiscountKlaim (FakturId, FakturDiscountId)
GO

-- ============================================================
-- File: \SalesContext\BTR_FakturItem.sql
-- ============================================================

CREATE  TABLE BTR_FakturItem(
    FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FakturItem_FakturId DEFAULT(''),
    FakturItemId VARCHAR(16) NOT NULL CONSTRAINT DF_BTR_FakturItem_FakturItemId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_FakturItem_ItemNo DEFAULT(''),
    
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_FakturItem_BrgId DEFAULT(''),
    BrgCode VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_FakturItem_BrgCode DEFAULT(''),
    StokHargaStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Faktur_StokHargaStr DEFAULT(''),
    QtyInputStr VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Faktur_QtyInputStr DEFAULT(''),
    QtyDetilStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_Faktur_QtyDetilStr DEFAULT(''),
    HrgInputStr VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_Faktur_HrgInputStr DEFAULT(''),

    QtyBesar INT NOT NULL CONSTRAINT DF_BTR_FakturItem_QtyBesar DEFAULT(0),
    SatBesar VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_FakturItem_SatBesar DEFAULT(''),
    Conversion INT NOT NULL CONSTRAINT DF_BTR_FakturItem_Conversion DEFAULT(1),
    HrgSatBesar DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItem_HargaSatBesar DEFAULT(0),

    QtyKecil INT NOT NULL CONSTRAINT DF_BTR_FakturItem_QtyKecil DEFAULT(0),
    SatKecil VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_FakturItem_SatKecil DEFAULT(''),
    HrgSatKecil DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemHrgSatKecil DEFAULT(''),

    QtyJual INT NOT NULL CONSTRAINT DF_BTR_FakturItem_QtyJual DEFAULT(0),
    HrgSat DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItem_HargaSatuan DEFAULT(0),
    SubTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItem_SubTotal DEFAULT(0),
    
    QtyBonus INT NOT NULL CONSTRAINT DF_BTR_FakturItem_QtyBonus DEFAULT(0),
    QtyPotStok INT NOT NULL CONSTRAINT DF_BTR_FakturItem_QtyPotStok DEFAULT(0),
    
    DiscInputStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FakturItem_DiscInputStr DEFAULT(''),
    DiscDetilStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FakturItem_DiscDetilStr DEFAULT(''),
    DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItem_DiscRp DEFAULT(0),
    
    DppProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItem_DppProsen DEFAULT(0),
    DppRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItem_DppRp DEFAULT(0),
    PpnProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItem_PpnProsen DEFAULT(0),
    PpnRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItem_PpnRp DEFAULT(0),
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItem_Total DEFAULT(0),

    CONSTRAINT PK_BTR_FakturItem PRIMARY KEY CLUSTERED(FakturItemId)
)
GO

CREATE INDEX IX_BTR_Faktur_FakturId
    ON BTR_FakturItem (FakturId, FakturItemId)
GO



-- ============================================================
-- File: \SalesContext\BTR_FakturItemKlaim.sql
-- ============================================================

CREATE  TABLE BTR_FakturItemKlaim(
    FakturId VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_FakturId DEFAULT(''),
    FakturItemId VARCHAR(16) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_FakturItemId DEFAULT(''),
    NoUrut INT NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_ItemNo DEFAULT(''),
    
    BrgId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_BrgId DEFAULT(''),
    BrgCode VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_BrgCode DEFAULT(''),
    StokHargaStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_StokHargaStr DEFAULT(''),
    QtyInputStr VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_QtyInputStr DEFAULT(''),
    QtyDetilStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_QtyDetilStr DEFAULT(''),
    HrgInputStr VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_HrgInputStr DEFAULT(''),

    QtyBesar INT NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_QtyBesar DEFAULT(0),
    SatBesar VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_SatBesar DEFAULT(''),
    Conversion INT NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_Conversion DEFAULT(1),
    HrgSatBesar DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_HargaSatBesar DEFAULT(0),

    QtyKecil INT NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_QtyKecil DEFAULT(0),
    SatKecil VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_SatKecil DEFAULT(''),
    HrgSatKecil DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_HrgSatKecil DEFAULT(''),

    QtyJual INT NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_QtyJual DEFAULT(0),
    HrgSat DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_HargaSatuan DEFAULT(0),
    SubTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_SubTotal DEFAULT(0),
    
    QtyBonus INT NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_QtyBonus DEFAULT(0),
    QtyPotStok INT NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_QtyPotStok DEFAULT(0),
    
    DiscInputStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_DiscInputStr DEFAULT(''),
    DiscDetilStr VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_DiscDetilStr DEFAULT(''),
    DiscRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_DiscRp DEFAULT(0),
    
    DppProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_DppProsen DEFAULT(0),
    DppRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_DppRp DEFAULT(0),
    PpnProsen DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_PpnProsen DEFAULT(0),
    PpnRp DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_PpnRp DEFAULT(0),
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_FakturItemKlaim_Total DEFAULT(0),

    CONSTRAINT PK_BTR_FakturItemKlaim PRIMARY KEY CLUSTERED(FakturItemId)
)
GO

CREATE INDEX IX_BTR_FakturItemKlaim_FakturId
    ON BTR_FakturItemKlaim (FakturId, FakturItemId)
GO



-- ============================================================
-- File: \SalesContext\BTR_FakturPajakVoid.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_FakturPajakVoid]
(
	NoFakturPajak VARCHAR(19) NOT NULL CONSTRAINT DF_BTR_FakturPajakVoid_NoFakturPajak DEFAULT(''),
	VoidDate DATETIME NOT NULL CONSTRAINT DF_BTR_FakturPajakVoid_VoidDate DEFAULT('3000-01-01'),
	AlasanVoid VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FakturPajakVoid_AlasanVoid DEFAULT(''),
	UserId VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_FakturPajakVoid_UserId DEFAULT(''),

	CONSTRAINT PK_FakturPajakVoid_NoFakturPajak PRIMARY KEY CLUSTERED (NoFakturPajak)
)


-- ============================================================
-- File: \SalesContext\BTR_HariRute.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_HariRute]
(
	HariRuteId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_HariRute_HariRuteId DEFAULT(''),
	HariRuteName VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_HariRute_HariRuteName DEFAULT(''),
	ShortName VARCHAR(8) NOT NULL CONSTRAINT DF_BTR_HariRUte_ShortRuteName DEFAULT(''),

	CONSTRAINT PK_BTR_HariRute  PRIMARY KEY CLUSTERED  (HariRuteId)
)


-- ============================================================
-- File: \SalesContext\BTR_HariRuteDataSeed.sql
-- ============================================================

INSERT INTO BTR_HariRute
SELECT 'H11', 'Minggu-1 Senin', 'Senin-1' UNION
SELECT 'H12', 'Minggu-1 Selasa', 'Selasa-1' UNION
SELECT 'H13', 'Minggu-1 Rabu', 'Rabu-1' UNION
SELECT 'H14', 'Minggu-1 Kamis', 'Kamis-1' UNION
SELECT 'H15', 'Minggu-1 Jumat', 'Jumat-1' UNION
SELECT 'H16', 'Minggu-1 Sabtu', 'Sabtu-1' UNION
SELECT 'H21', 'Minggu-2 Senin', 'Senin-2' UNION
SELECT 'H22', 'Minggu-2 Selasa', 'Selasa-2' UNION
SELECT 'H23', 'Minggu-2 Rabu', 'Rabu-2' UNION
SELECT 'H24', 'Minggu-2 Kamis', 'Kamis-2' UNION
SELECT 'H25', 'Minggu-2 Jumat', 'Jumat-2' UNION
SELECT 'H26', 'Minggu-2 Sabtu', 'Sabtu-2'

-- ============================================================
-- File: \SalesContext\BTR_Klasifikasi.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_Klasifikasi]
(
	KlasifikasiId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_Klasifikasi_KlasifikasiId DEFAULT(''),
	KlasifikasiName VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Klasifikasi_KlasifikasiName DEFAULT(''),

	CONSTRAINT PK_BTR_Klasifikasi PRIMARY KEY CLUSTERED (KlasifikasiId)
)

-- ============================================================
-- File: \SalesContext\BTR_OrderMap.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_OrderMap]
(
    OrderId VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_OrderMap_OrderId DEFAULT(''),
    FakturId VARCHAR(15) NOT NULL CONSTRAINT DF_BTR_OrderMap_FakturId DEFAULT(''),
    FakturCode VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_OrderMap_FakturCode DEFAULT(''),
    UserName VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_OrderMap_UserName DEFAULT(''),
    Timestamp DATETIME NOT NULL CONSTRAINT DF_BTR_OrderMap_Timestamp DEFAULT('3000-01-01')

    CONSTRAINT PK_BTR_OrderMap PRIMARY KEY CLUSTERED(OrderId)
)

-- ============================================================
-- File: \SalesContext\BTR_SalesOmzet.sql
-- ============================================================

CREATE TABLE BTR_SalesOmzet
(
    SalesOmzetId     VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_SalesOmzetId DEFAULT(''),
    OrderId          VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_OrderId DEFAULT(''),
    FakturId         VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_FakturId DEFAULT(''),
    SaleKind         VARCHAR(15) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_SaleKind DEFAULT(''),

    SalesDate        DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_SalesDate DEFAULT('3000-01-01'),
    OmzetDate        DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_OmzetDate DEFAULT('3000-01-01'),

    SalesPersonName  VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_SalesPersonName DEFAULT(''),
    OrderDate        DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_OrderDate DEFAULT('3000-01-01'),
    OrderTotal       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_OrderTotal DEFAULT(0),
    FakturCode       VARCHAR(11) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_FakturCode DEFAULT(''),
    FakturDate       DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_FakturDate DEFAULT('3000-01-01'),
    FakturTotal      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_FakturTotal DEFAULT(0),
    CustomerName     VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_CustomerName DEFAULT(''),
    Code             VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_Code DEFAULT(''),
    Alamat           VARCHAR(60) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_Alamat DEFAULT(''),
    OmzetStatus      VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_OmzetStatus DEFAULT(''),

    CreatedAt        DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_CreatedAt DEFAULT('3000-01-01'),
    LastReconciledAt DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_LastReconciledAt DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTR_SalesOmzet PRIMARY KEY CLUSTERED (SalesOmzetId)
)
GO

CREATE UNIQUE INDEX UX_BTR_SalesOmzet_OrderId
    ON BTR_SalesOmzet (OrderId)
    WHERE OrderId <> ''
GO

CREATE UNIQUE INDEX UX_BTR_SalesOmzet_FakturId
    ON BTR_SalesOmzet (FakturId)
    WHERE FakturId <> ''
GO

CREATE INDEX IX_BTR_SalesOmzet_SalesDate
    ON BTR_SalesOmzet (SalesDate, SalesOmzetId)
GO

CREATE INDEX IX_BTR_SalesOmzet_OmzetDate
    ON BTR_SalesOmzet (OmzetDate, SalesOmzetId)
GO


-- ============================================================
-- File: \SalesContext\BTR_SalesOmzetHealthWeekly.sql
-- ============================================================

CREATE TABLE BTR_SalesOmzetHealthWeekly
(
    HealthWeeklyId         VARCHAR(13)  NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_HealthWeeklyId DEFAULT(''),
    YearNumber             INT          NOT NULL,
    WeekNumber             INT          NOT NULL,
    PeriodStartDate        DATETIME     NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_PeriodStartDate DEFAULT('3000-01-01'),
    PeriodEndDate          DATETIME     NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_PeriodEndDate DEFAULT('3000-01-01'),
    HealthLevel            VARCHAR(10)  NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_HealthLevel DEFAULT(''),
    HealthScore            INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_HealthScore DEFAULT(0),
    MissingOrdersCount     INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_MissingOrdersCount DEFAULT(0),
    MissingFaktursCount    INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_MissingFaktursCount DEFAULT(0),
    UnlinkedFaktursCount   INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_UnlinkedFaktursCount DEFAULT(0),
    StaleDataCount         INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_StaleDataCount DEFAULT(0),
    LastCalculatedAt       DATETIME     NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_LastCalculatedAt DEFAULT('3000-01-01'),
    CalculationDurationMs  INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_CalculationDurationMs DEFAULT(0),
    CreatedAt              DATETIME     NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_CreatedAt DEFAULT('3000-01-01'),
    UpdatedAt              DATETIME     NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_UpdatedAt DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTR_SalesOmzetHealthWeekly PRIMARY KEY CLUSTERED (HealthWeeklyId),
    CONSTRAINT UX_BTR_SalesOmzetHealthWeekly_YearWeek UNIQUE (YearNumber, WeekNumber)
)
GO

CREATE INDEX IX_BTR_SalesOmzetHealthWeekly_YearWeek
    ON BTR_SalesOmzetHealthWeekly (YearNumber, WeekNumber)
GO


-- ============================================================
-- File: \SalesContext\BTR_SalesOmzetTarget.sql
-- ============================================================

CREATE TABLE BTR_SalesOmzetTarget
(
    SalesPersonId VARCHAR(5)  NOT NULL,
    TargetYear    INT         NOT NULL,
    TargetMonth   INT         NOT NULL,
    TargetAmount  DECIMAL(18, 2) NOT NULL CONSTRAINT DF_BTR_SalesOmzetTarget_TargetAmount DEFAULT (0),

    CONSTRAINT PK_BTR_SalesOmzetTarget PRIMARY KEY CLUSTERED (SalesPersonId, TargetYear, TargetMonth),
    CONSTRAINT FK_BTR_SalesOmzetTarget_SalesPerson
        FOREIGN KEY (SalesPersonId) REFERENCES BTR_SalesPerson (SalesPersonId)
)
GO

CREATE INDEX IX_BTR_SalesOmzetTarget_YearMonth
    ON BTR_SalesOmzetTarget (TargetYear, TargetMonth)
GO


-- ============================================================
-- File: \SalesContext\BTR_SalesPerson.sql
-- ============================================================

CREATE TABLE BTR_SalesPerson(
    SalesPersonId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_SalesPerson_SalesPersonId DEFAULT(''),
    SalesPersonCode VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_SalesPerson_SalesPersonCode DEFAULT(''),
    SalesPersonName VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_SalesPerson_SalesPersonName DEFAULT(''),
    WilayahId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_SalesPerson_WilayahId DEFAULT(''),
    Email VARCHAR(100) NULL CONSTRAINT DF_BTR_SalesPerson_Email DEFAULT(''),
    SegmentId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_SalesPerson_SegmentId DEFAULT(''),

    CONSTRAINT PK_BTR_SalesPerson PRIMARY KEY CLUSTERED(SalesPersonId)
)

-- ============================================================
-- File: \SalesContext\BTR_SalesPersonPrincipalTarget.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_SalesPersonPrincipalTarget]
(
	SalesPersonId VARCHAR(5)  NOT NULL,
	SupplierId    VARCHAR(5)  NOT NULL,
	TargetYear    INT         NOT NULL,
	TargetMonth   INT         NOT NULL,
	TargetAmount  DECIMAL(18, 2) NOT NULL
		CONSTRAINT DF_BTR_SalesPersonPrincipalTarget_TargetAmount DEFAULT (0),
	UpdatedDate   DATETIME    NOT NULL
		CONSTRAINT DF_BTR_SalesPersonPrincipalTarget_UpdatedDate DEFAULT (GETDATE()),

	CONSTRAINT PK_BTR_SalesPersonPrincipalTarget
		PRIMARY KEY CLUSTERED (SalesPersonId, SupplierId, TargetYear, TargetMonth),

	CONSTRAINT FK_BTR_SalesPersonPrincipalTarget_SalesPerson
		FOREIGN KEY (SalesPersonId) REFERENCES BTR_SalesPerson (SalesPersonId),

	CONSTRAINT FK_BTR_SalesPersonPrincipalTarget_Supplier
		FOREIGN KEY (SupplierId) REFERENCES BTR_Supplier (SupplierId),

	CONSTRAINT CK_BTR_SalesPersonPrincipalTarget_TargetMonth
		CHECK (TargetMonth BETWEEN 1 AND 12),

	CONSTRAINT CK_BTR_SalesPersonPrincipalTarget_TargetAmount
		CHECK (TargetAmount >= 0)
)
GO

CREATE INDEX IX_BTR_SalesPersonPrincipalTarget_YearMonth
	ON BTR_SalesPersonPrincipalTarget (TargetYear, TargetMonth)
	INCLUDE (SalesPersonId, SupplierId, TargetAmount)
GO

CREATE INDEX IX_BTR_SalesPersonPrincipalTarget_SalesPerson_Period
	ON BTR_SalesPersonPrincipalTarget (SalesPersonId, TargetYear, TargetMonth)
	INCLUDE (SupplierId, TargetAmount)
GO


-- ============================================================
-- File: \SalesContext\BTR_SalesPersonSupplier.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_SalesPersonSupplier]
(
	SalesPersonId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_SalesPersonSupplier_SalesPersonId DEFAULT(''),
	SupplierId    VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_SalesPersonSupplier_SupplierId DEFAULT(''),

	CONSTRAINT PK_BTR_SalesPersonSupplier
		PRIMARY KEY CLUSTERED (SalesPersonId, SupplierId),
	CONSTRAINT FK_BTR_SalesPersonSupplier_SalesPerson
		FOREIGN KEY (SalesPersonId) REFERENCES BTR_SalesPerson (SalesPersonId),
	CONSTRAINT FK_BTR_SalesPersonSupplier_Supplier
		FOREIGN KEY (SupplierId) REFERENCES BTR_Supplier (SupplierId)
)
GO


-- ============================================================
-- File: \SalesContext\BTR_SalesRute.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_SalesRute]
(
	SalesRuteId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_SalesRute_SalesRuteId DEFAULT(''),
	SalesPersonId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_SalesRute_SalesPersonId DEFAULT(''),
	HariRuteId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_SalesRute_HariRuteId DEFAULT(''),

	CONSTRAINT PK_BTR_SalesRute PRIMARY KEY CLUSTERED(SalesRuteId)
)
GO

CREATE UNIQUE INDEX IX_BTR_SalesRute_SalesPersonId
	ON [dbo].[BTR_SalesRute](SalesPersonId, HariRuteId)
GO


-- ============================================================
-- File: \SalesContext\BTR_SalesRuteItem.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_SalesRuteItem]
(
	SalesRuteId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_SalesRuteItem_SalesRuteId DEFAULT(''),
	NoUrut INT NOT NULL CONSTRAINT DF_BTR_SalesRuteItem_NoUrut DEFAULT(0),
	CustomerId VARCHAR(6) NOT NULL CONSTRAINT DF_BTR_SalesRuteItem_CustomerId DEFAULT(''),

	CONSTRAINT PK_BTR_SalesRuteItem PRIMARY KEY CLUSTERED (SalesRuteId, NoUrut)
)
GO

CREATE UNIQUE INDEX UX_BTR_SalesRuteItem_CustomerId
	ON BTR_SalesRuteItem (SalesRuteId, CustomerId)
GO


-- ============================================================
-- File: \SalesContext\BTR_Segment.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_Segment]
(
	SegmentId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_Segment_SegmentId DEFAULT (''),
	SegmentName VARCHAR(255) NOT NULL CONSTRAINT DF_BTR_Segment_SegmentName DEFAULT (''),

	CONSTRAINT PK_BTR_Segment PRIMARY KEY CLUSTERED (SegmentId)
)
GO

--INSERT INTO BTR_Segment (SegmentId, SegmentName) 
--SELECT 'GT', 'General Trade' UNION ALL
--SELECT 'MM', 'Mini Market' UNION ALL
--SELECT 'SPM', 'Supermarket' UNION ALL
--SELECT 'NKA', 'National Key Account'
--GO


-- ============================================================
-- File: \SalesContext\BTR_VisitPlan.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_VisitPlan]
(
	VisitPlanId     VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_VisitPlan_VisitPlanId DEFAULT(''),
	SalesPersonId   VARCHAR(5)  NOT NULL CONSTRAINT DF_BTR_VisitPlan_SalesPersonId DEFAULT(''),
	VisitDate       DATE        NOT NULL,
	CustomerId      VARCHAR(6)  NOT NULL CONSTRAINT DF_BTR_VisitPlan_CustomerId DEFAULT(''),
	NoUrut          INT         NOT NULL CONSTRAINT DF_BTR_VisitPlan_NoUrut DEFAULT(0),
	HariRuteId      VARCHAR(3)  NOT NULL CONSTRAINT DF_BTR_VisitPlan_HariRuteId DEFAULT(''),
	PlanSource      VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_VisitPlan_PlanSource DEFAULT('Template'),
	MaterializedAt  DATETIME    NOT NULL,

	CONSTRAINT PK_BTR_VisitPlan PRIMARY KEY CLUSTERED (VisitPlanId),
	CONSTRAINT UX_BTR_VisitPlan UNIQUE (SalesPersonId, VisitDate, CustomerId)
)
GO

CREATE INDEX IX_BTR_VisitPlan_VisitDate
	ON [dbo].[BTR_VisitPlan] (VisitDate)
GO

CREATE INDEX IX_BTR_VisitPlan_SalesPersonDate
	ON [dbo].[BTR_VisitPlan] (SalesPersonId, VisitDate)
GO


-- ============================================================
-- File: \SalesContext\BTR_VisitPlanException.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_VisitPlanException]
(
	VisitPlanExceptionId  VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_VisitPlanException_VisitPlanExceptionId DEFAULT(''),
	SalesPersonId         VARCHAR(5)  NOT NULL CONSTRAINT DF_BTR_VisitPlanException_SalesPersonId DEFAULT(''),
	VisitDate             DATE        NOT NULL,
	ExceptionType         VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_VisitPlanException_ExceptionType DEFAULT(''),
	CustomerId            VARCHAR(6)  NOT NULL CONSTRAINT DF_BTR_VisitPlanException_CustomerId DEFAULT(''),
	ReplacementCustomerId VARCHAR(6)  NOT NULL CONSTRAINT DF_BTR_VisitPlanException_ReplacementCustomerId DEFAULT(''),
	CreatedAt             DATETIME    NOT NULL,
	CreatedByUserId       VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_VisitPlanException_CreatedByUserId DEFAULT(''),

	CONSTRAINT PK_BTR_VisitPlanException PRIMARY KEY CLUSTERED (VisitPlanExceptionId)
)
GO

CREATE INDEX IX_BTR_VisitPlanException_Lookup
	ON [dbo].[BTR_VisitPlanException] (SalesPersonId, VisitDate)
GO


-- ============================================================
-- File: \SalesContext\BTR_Wilayah.sql
-- ============================================================

CREATE TABLE [dbo].[BTR_Wilayah]
(
	WilayahId VARCHAR(3) NOT NULL CONSTRAINT DF_BTR_Wilayah_WilayahId DEFAULT(''),
	WilayahName VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_Wilayah_WilayahName DEFAULT(''),

	CONSTRAINT PK_BTR_Wilayah PRIMARY KEY CLUSTERED (WilayahId)
)

