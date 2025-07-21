using Azunt.Repositories;

namespace Azunt.TechnicianManagement;

/// <summary>
/// 기본 CRUD 작업을 위한 Technician 전용 저장소 인터페이스
/// </summary>
public interface ITechnicianBaseRepository : IRepositoryBase<Technician, long>
{
}