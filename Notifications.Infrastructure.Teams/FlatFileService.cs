using Microsoft.AspNetCore.Hosting;
using Notifications.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Teams
{
    public class FlatFileService
    {
        private readonly IHostingEnvironment _env;
        public FlatFileService(IHostingEnvironment env)
        {
            _env = env;
        }
        public void SaveInFile(string fileName, string data)
        {
            var fullFileName = _env.ContentRootPath + Path.DirectorySeparatorChar 
                + "ChatsTeamsGroups" + Path.DirectorySeparatorChar + fileName;
            using (var stream = File.Open(fullFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                string fecha = DateTime.Now.ToLongTimeString();
                byte[] info = new UTF8Encoding(true).GetBytes(data);
                stream.Write(info, 0, info.Length);
                stream.Close();
            }
        }

        public string FindOnFile(string fileName, List<string> members) {
            var fullFileName = _env.ContentRootPath + Path.DirectorySeparatorChar
                + "ChatsTeamsGroups" + Path.DirectorySeparatorChar + fileName;


            var filters = new List<FilterDescriptor>();
            members.ForEach(member => filters.Add(new FilterDescriptor("", member, "contains")));
            var filter = BuildExpressionHelper.GetFilter<string>(filters);

            var lines = File.ReadAllLines(fullFileName).Where(filter.Compile());
            var line = lines.FirstOrDefault(x => x.Split(';')[1].Split('|').Count() == members.Count());
            return line;
        }

        public bool Exists(string fileName) {
            var fullFileName = _env.ContentRootPath + Path.DirectorySeparatorChar
                + "ChatsTeamsGroups" + Path.DirectorySeparatorChar + fileName;

            return File.Exists(fullFileName);
        }

    }
}
