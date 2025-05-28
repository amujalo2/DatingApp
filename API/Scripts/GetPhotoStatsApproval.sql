CREATE PROCEDURE GetPhotoStatsApproval
    @CurrentUserId NVARCHAR(450)
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM AspNetUserRoles as UR
        JOIN AspNetRoles as R ON UR.RoleId = R.Id
        WHERE UR.UserId = @CurrentUserId AND R.Name = 'Admin'
    )
    BEGIN
        SELECT 
            U.UserName,
            COUNT(CASE WHEN P.IsApproved = 1 THEN 1 END) AS APPROVED_PHOTOS,
            COUNT(CASE WHEN P.IsApproved = 0 THEN 1 END) AS UNAPPROVED_PHOTOS
        FROM AspNetUsers as U
        LEFT JOIN Photos as P ON U.Id = P.AppUserId
        GROUP BY U.UserName;
    END
    ELSE
    BEGIN
        RAISERROR('Access denied. Administrative privileges required.', 16, 1);
    END
END;
