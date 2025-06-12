namespace API.Helpers;

public class LikesParams : PaginationParams
{
    public int UserID { get; set; }
    public LikesPredicate Predicate { get; set; }
}
