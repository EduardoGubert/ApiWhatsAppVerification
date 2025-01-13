using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsAppVerification.Application.UseCases
{
    public class InstanceRotatorUseCase
    {
        private readonly List<string> _instances;
        private int _currentIndex = -1;
        private readonly object _lock = new object();

        public InstanceRotatorUseCase(IConfiguration configuration)
        {
            _instances = configuration.GetSection("EvolutionApi:Instances").Get<List<string>>();
        }

        public string GetNextInstance()
        {
            lock (_lock)
            {
                _currentIndex = (_currentIndex + 1) % _instances.Count;
                return _instances[_currentIndex];
            }
        }
    }
}
