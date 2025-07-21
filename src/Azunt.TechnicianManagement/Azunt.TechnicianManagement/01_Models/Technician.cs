using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Azunt.TechnicianManagement
{
    /// <summary>
    /// Technicians 테이블과 매핑되는 테크니션(Technician) 엔터티 클래스입니다.
    /// </summary>
    [Table("Technicians")]
    public class Technician
    {
        /// <summary>
        /// 테크니션 고유 아이디 (자동 증가)
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// 활성 상태 (기본값: true)
        /// </summary>
        public bool? Active { get; set; }

        /// <summary>
        /// 소프트 삭제 플래그 (기본값: false)
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 생성 일시
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// 생성자 이름
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 테크니션 이름
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? Name { get; set; }

        /// <summary>
        /// 정렬 순서
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}