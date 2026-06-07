CREATE INDEX IX_BTR_Piutang_OpenBalance
    ON [dbo].[BTR_Piutang] (Sisa, PiutangId)
    INCLUDE (DueDate, Total, CustomerId)
    WHERE Sisa > 1
GO
