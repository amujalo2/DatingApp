using System;

namespace API.DTOs;

public class PhotoApprovalStatisticsDto
{
    public string? Username { get; set; }
    public int ApprovedPhotos { get; set; }
    public int UnapprovedPhotos { get; set; }
}
