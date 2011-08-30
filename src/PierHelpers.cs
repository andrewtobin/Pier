using System.Configuration;
using System.Xml.Linq;
using Nancy;

namespace Pier
{
    public static class PierHelpers
    {
        public static Response ToXml(this Shortened shortened)
        {
            return new Response
                       {
                           ContentType = "application/xml",
                           Contents = stream =>
                                          {
                                              var x = new XElement("result",
                                                                   new XElement("url",
                                                                                new XElement("id", shortened.Id),
                                                                                new XElement("keyword", shortened.ShortUrl),
                                                                                new XElement("url", shortened.LongUrl),
                                                                                new XElement("date", shortened.Date)
                                                                       //new XElement("ip", ip)
                                                                       ),
                                                                   new XElement("status", "success"),
                                                                   new XElement("message", string.Format("{0} added to database", shortened.LongUrl)),
                                                                   new XElement("shorturl", /*should add in base url*/
                                                                                shortened.ShortUrl)
                                                  );
                                              x.Save(stream);
                                          }
                       };

        }


        private static string _shortUrlCharacterSet;
        public static string ShortUrlCharacterSet
        {
            get
            {
                if (_shortUrlCharacterSet == null)
                {
                    switch (ConfigurationManager.AppSettings["baseHash"])
                    {
                        case "36":
                            _shortUrlCharacterSet = "0123456789abcdefghijklmnopqrstuvwxyz";
                            break;
                        case "62":
                        case "64":
                            _shortUrlCharacterSet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                            break;

                    }
                }

                return _shortUrlCharacterSet;
            }
        }

        public static string Encode(int id)
        {
            string result = string.Empty;
            var length = ShortUrlCharacterSet.Length;

            while (id >= length)
            {
                var mod = id % length;
                id = id / length;
                result = ShortUrlCharacterSet[mod] + result;
            }

            result = ShortUrlCharacterSet[id] + result;
            return result;
        }

    }
}