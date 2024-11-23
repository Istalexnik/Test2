using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppServerTest2;
public interface IAlertService
{
    Task ShowAlertAsync(string title, string message, string cancelButton);
}