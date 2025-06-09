CREATE PROCEDURE GetUsersWithoutMainPhoto
    @CurrentUserId NVARCHAR(450) 
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM AspNetUserRoles ur
        JOIN AspNetRoles r ON ur.RoleId = r.Id
        WHERE ur.UserId = @CurrentUserId AND r.Name = 'Admin'
    )
    BEGIN
        SELECT u.UserName AS USERNAME
        FROM AspNetUsers u
        WHERE NOT EXISTS (
            SELECT 1
            FROM Photos p
            WHERE p.AppUserId = u.Id AND p.IsMain = 1
        );
    END
    ELSE
    BEGIN
        RAISERROR('Access denied. Administrative privileges required.', 16, 1);
    END
END;
