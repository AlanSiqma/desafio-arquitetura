using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class IdempotencyKey
    {
        public Guid Id { get; set; }

        public string Key { get; set; } = string.Empty;

        public string RequestHash { get; set; } = string.Empty;

        public Guid LancamentoId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
