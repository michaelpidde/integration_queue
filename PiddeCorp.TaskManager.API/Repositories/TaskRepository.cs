using Dapper;
using Microsoft.Data.SqlClient;
using PiddeCorp.TaskManager.API.Models;

namespace PiddeCorp.TaskManager.API.Repositories;

public class TaskRepository(IConfiguration configuration) {
    private readonly string _connectionString = configuration.GetConnectionString("SqlServer")!;
    
    public IEnumerable<OpenTaskDto> GetOpenTasks() {
        using SqlConnection conn = new(_connectionString);
        return conn.Query<OpenTaskDto>("""
            SELECT 
                t.Id,
                t.Created,
                t.Type,
                t.Due,
                ws.Name AS WorkflowStatus,
                u.Email AS AssignedTo
            FROM OpenTask t
            JOIN OpenTaskWorkflow otw ON otw.OpenTaskId = t.Id
            JOIN WorkflowStatus ws ON ws.Id = otw.WorkflowStatusId
            LEFT JOIN [User] u ON u.Id = otw.UserId
        """
        );
    }
}
