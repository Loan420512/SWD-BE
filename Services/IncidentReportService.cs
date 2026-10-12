using Microsoft.EntityFrameworkCore;
using StickerRobot.Constants;
using StickerRobot.Data;
using StickerRobot.DTO.DTORequest;
using StickerRobot.DTO.DTOResponse;
using StickerRobot.Models;
using StickerRobot.Services.Interfaces;

namespace StickerRobot.Services;

public class IncidentReportService : IIncidentReportService
{
    private readonly StickerRobotDbContext _context;

    public IncidentReportService(StickerRobotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IncidentReportResponse>> GetAllAsync()
    {
        return await _context.IncidentReports
            .AsNoTracking()
            .Select(i => new IncidentReportResponse
            {
                Id = i.Id,
                StaffId = i.StaffId,
                IncidentType = i.IncidentType,
                Description = i.Description,
                Status = i.Status,
                ReportedAt = i.ReportedAt
            })
            .ToListAsync();
    }

    public async Task<IncidentReportResponse?> GetByIdAsync(int id)
    {
        var i = await _context.IncidentReports.FindAsync(id);
        if (i == null) return null;

        return new IncidentReportResponse
        {
            Id = i.Id,
            StaffId = i.StaffId,
            IncidentType = i.IncidentType,
            Description = i.Description,
            Status = i.Status,
            ReportedAt = i.ReportedAt
        };
    }

    public async Task<(IncidentReportResponse? Data, string? ErrorMessage)> CreateAsync(IncidentReportCreateRequest request)
    {
        if (request.StaffId.HasValue && !await _context.Staff.AnyAsync(s => s.Id == request.StaffId))
        {
            return (null, $"Staff with ID {request.StaffId} does not exist.");
        }

        var report = new IncidentReport
        {
            StaffId = request.StaffId,
            IncidentType = request.IncidentType,
            Description = request.Description,
            Status = string.IsNullOrWhiteSpace(request.Status) ? IncidentStatus.Open : request.Status,
            ReportedAt = DateTime.UtcNow
        };

        _context.IncidentReports.Add(report);
        await _context.SaveChangesAsync();

        var response = new IncidentReportResponse
        {
            Id = report.Id,
            StaffId = report.StaffId,
            IncidentType = report.IncidentType,
            Description = report.Description,
            Status = report.Status,
            ReportedAt = report.ReportedAt
        };

        return (response, null);
    }

    public async Task<(IncidentReportResponse? Data, string? ErrorMessage)> UpdateAsync(int id, IncidentReportUpdateRequest request)
    {
        var report = await _context.IncidentReports.FindAsync(id);
        if (report == null) return (null, null);

        if (request.StaffId.HasValue && !await _context.Staff.AnyAsync(s => s.Id == request.StaffId))
        {
            return (null, $"Staff with ID {request.StaffId} does not exist.");
        }

        if (request.StaffId.HasValue) report.StaffId = request.StaffId;
        if (request.IncidentType != null) report.IncidentType = request.IncidentType;
        if (request.Description != null) report.Description = request.Description;
        if (request.Status != null) report.Status = request.Status;

        await _context.SaveChangesAsync();

        var response = new IncidentReportResponse
        {
            Id = report.Id,
            StaffId = report.StaffId,
            IncidentType = report.IncidentType,
            Description = report.Description,
            Status = report.Status,
            ReportedAt = report.ReportedAt
        };

        return (response, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var report = await _context.IncidentReports.FindAsync(id);
        if (report == null) return false;

        _context.IncidentReports.Remove(report);
        await _context.SaveChangesAsync();
        return true;
    }
}
