using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Service_NewsToday_Api
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer = null;
        dynamic service_location = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            File.AppendAllText($"{service_location}\\Log_Debug.txt", "OnStart" + $"{System.Environment.NewLine}");


            //as service starts intiallizing timer
            timer = new Timer();
            timer.Interval = 60000;//3600000;

            //enabling timer
            timer.Enabled = true;
            File.AppendAllText($"{service_location}\\Log_Debug.txt", "About to call event" + $"{System.Environment.NewLine}");


            timer.Elapsed += FetchDataFromNewsApi;
        }

        private void FetchDataFromNewsApi(object sender, ElapsedEventArgs e)
        {
            File.AppendAllText($"{service_location}\\Log_Debug.txt", "In Function" + $"{System.Environment.NewLine}");
            var connectionstr = ConfigurationManager.ConnectionStrings["connection"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionstr);

            //Delete from tables to fill new data
            SqlCommand sqlCommand;
            try
            {
                sqlCommand = new SqlCommand("DELETE FROM dbo.Sports", conn);
                conn.Open();
                int y = sqlCommand.ExecuteNonQuery();

                if (y > 0)
                {
                    File.AppendAllText($"{service_location}\\Log_Debug.txt", "Old Data Sports Table Deleted" + $"{System.Environment.NewLine}");

                }
                else
                {
                    File.AppendAllText($"{service_location}\\Log_Debug.txt", "Failed : Old Data Sports Table Deleted" + $"{System.Environment.NewLine}");
                }

                

                sqlCommand = new SqlCommand("DELETE FROM dbo.Entertainment", conn);
                y = sqlCommand.ExecuteNonQuery();
                if (y > 0)
                {
                    File.AppendAllText($"{service_location}\\Log_Debug.txt", "Old Data Entertainment Table Deleted" + $"{System.Environment.NewLine}");
                }
                else
                {
                    File.AppendAllText($"{service_location}\\Log_Debug.txt", "Failed : Old Data Entertainment Table Deleted" + $"{System.Environment.NewLine}");
                }

                sqlCommand = new SqlCommand("DELETE FROM dbo.General", conn);
                y = sqlCommand.ExecuteNonQuery();
                if (y > 0)
                {
                    File.AppendAllText($"{service_location}\\Log_Debug.txt", "Old Data General Table Deleted" + $"{System.Environment.NewLine}");
                }
                else
                {
                    File.AppendAllText($"{service_location}\\Log_Debug.txt", "Failed :Old Data Sports Table Deleted" + $"{System.Environment.NewLine}");
                }


                sqlCommand = new SqlCommand("DELETE FROM dbo.News", conn);
                y = sqlCommand.ExecuteNonQuery();
                if (y > 0)
                {
                    File.AppendAllText($"{service_location}\\Log_Debug.txt", "Old Data News Table Deleted" + $"{System.Environment.NewLine}");
                }
                else
                {
                    File.AppendAllText($"{service_location}\\Log_Debug.txt", "Failed : Old Data News Table Deleted" + $"{System.Environment.NewLine}");
                }

            }
            catch (Exception ex)
            {
                File.AppendAllText($"{service_location}\\Log_Debug.txt", ""+ ex.Message + $"{System.Environment.NewLine}");

            }
            finally
            {
                conn.Close();
            }


            //Now Insertion of Data

            int sports_id = 0;
            int entertainment_id = 0;
            int general_id = 0;
            int all_news_count = 0;
            // init with your API key

            var newsApiClient = new NewsApiClient("3f60c1e6a9e14fb8b3da93836553b367");
            var articlesResponse = newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
            {
                Country = Countries.IN,
                Category = Categories.Sports
            });
            int x = 0;

            if (articlesResponse.Status == Statuses.Ok)
            {
                // total results found
                //  Console.WriteLine(articlesResponse.TotalResults);
                // here's the first 20
                foreach (var article in articlesResponse.Articles)
                {
                    string title = article.Title;
                    string author = article.Author;
                    string desp = article.Description;
                    string urltoimg = article.UrlToImage;
                    string content = article.Content;
                    string URL = article.Url;
                
                    DateTime? publishedAt = article.PublishedAt;
                    if (title != null && author != null && desp != null && urltoimg != null && content != null && URL!=null)
                    {
                        SqlCommand cmd;
                        try
                        {

                            all_news_count++;
                            cmd = new SqlCommand("INSERT INTO dbo.News (NewsID,Category) VALUES (@n,@c)", conn);
                            cmd.Parameters.Add("@n", all_news_count);
                            cmd.Parameters.Add("@c", "Sports");
                            conn.Open();

                            x = cmd.ExecuteNonQuery();
                            if (x > 0)
                            {
                                File.AppendAllText($"{service_location}\\Log_Debug.txt", "NewsIds filled" + $"{System.Environment.NewLine}");
                            }
                            else
                            {
                                File.AppendAllText($"{service_location}\\Log_Debug.txt", "Failed to fill news id" + $"{System.Environment.NewLine}");
                            }



                            sports_id++;
                            cmd = new SqlCommand("INSERT INTO dbo.Sports (NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL) VALUES (@n,@t,@a,@d,@u,@c,@p,@fk,@ur)", conn);
                            cmd.Parameters.Add("@n", sports_id);
                            cmd.Parameters.Add("@t", title);
                            cmd.Parameters.Add("@a", author);
                            cmd.Parameters.Add("@d", desp);
                            cmd.Parameters.Add("@u", urltoimg);
                            if (content.Length < 260)
                            {
                                cmd.Parameters.Add("@c", content.Substring(0, content.Length));
                            }
                            else
                            {
                                cmd.Parameters.Add("@c", content.Substring(0, 260));

                            }
                            cmd.Parameters.Add("@p", publishedAt);
                            cmd.Parameters.Add("@fk",all_news_count);
                            cmd.Parameters.Add("@ur", URL);
                            x = cmd.ExecuteNonQuery();
                            if (x > 0)
                            {

                                File.AppendAllText($"{service_location}\\Log_Debug.txt", "Sport Table Filled" + $"{System.Environment.NewLine}");
                            }
                            else
                            {
                                File.AppendAllText($"{service_location}\\Log_Debug.txt", "Sport Table insertion failed" + $"{System.Environment.NewLine}");
                            }


                        }
                        catch (Exception ex)
                        {
                            all_news_count--;
                            sports_id--;
                            File.AppendAllText($"{service_location}\\Log_Debug.txt", "Sport Failure query : "+ex.Message + $"{System.Environment.NewLine}");
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }


            }
            //for entertainment news
            articlesResponse = newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
            {

                Category = Categories.Entertainment,
                Country = Countries.IN
            });

            x = 0;
            if (articlesResponse.Status == Statuses.Ok)
            {

                foreach (var article in articlesResponse.Articles)
                {
                    string title = article.Title;
                    string author = article.Author;
                    string desp = article.Description;
                    string urltoimg = article.UrlToImage;
                    string content = article.Content;
                    string URL = article.Url;
                    DateTime? publishedAt = article.PublishedAt;
                    if (title != null && author != null && desp != null && urltoimg != null && content != null && URL != null)
                    {
                        SqlCommand cmd;
                        try
                        {
                            all_news_count++;

                            cmd = new SqlCommand("INSERT INTO dbo.News (NewsID,Category) VALUES (@n,@c)", conn);
                            cmd.Parameters.Add("@n", all_news_count);
                            cmd.Parameters.Add("@c", "Entertainment");
                            conn.Open();
                            x = cmd.ExecuteNonQuery();
                            if (x > 0)
                            {
                                File.AppendAllText($"{service_location}\\Log_Debug.txt", "NewsID Filled" + $"{System.Environment.NewLine}");
                            }
                            else
                            {
                                File.AppendAllText($"{service_location}\\Log_Debug.txt", "News Id filled failed" + $"{System.Environment.NewLine}");
                            }

                            entertainment_id++;
                            cmd = new SqlCommand("INSERT INTO dbo.Entertainment (NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,URL) VALUES (@n,@t,@a,@d,@u,@c,@p,@fk,@ur)", conn);
                            cmd.Parameters.Add("@n", entertainment_id);
                            cmd.Parameters.Add("@t", title);
                            cmd.Parameters.Add("@a", author);
                            cmd.Parameters.Add("@d", desp);
                            cmd.Parameters.Add("@u", urltoimg);

                            
                            if (content.Length < 260)
                            {
                                cmd.Parameters.Add("@c", content.Substring(0, content.Length));
                            }
                            else
                            {
                                cmd.Parameters.Add("@c", content.Substring(0, 260));

                            }
                            cmd.Parameters.Add("@p", publishedAt);
                            cmd.Parameters.Add("@fk", all_news_count);
                            cmd.Parameters.Add("@ur", URL);
                             x = cmd.ExecuteNonQuery();

                            if (x > 0)
                            {
                                File.AppendAllText($"{service_location}\\Log_Debug.txt", "Entertainment Filled" + $"{System.Environment.NewLine}");
                            }
                            else
                            {
                                File.AppendAllText($"{service_location}\\Log_Debug.txt", "Entertainment Table Interstion failed" + $"{System.Environment.NewLine}");
                            }
                        }
                        catch (Exception ex)
                        {
                            entertainment_id--;
                            all_news_count--;
                            File.AppendAllText($"{service_location}\\Log_Debug.txt", "Query Failed : "+ex.Message + $"{System.Environment.NewLine}");
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }



                }
            }

            //for entertainment General
            articlesResponse = newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
            {
                Country = Countries.US
            });

            x = 0;
            if (articlesResponse.Status == Statuses.Ok)
            {
                foreach (var article in articlesResponse.Articles)
                {
                    string title = article.Title;
                    string author = article.Author;
                    string desp = article.Description;
                    string urltoimg = article.UrlToImage;
                    string content = article.Content;
                    DateTime? publishedAt = article.PublishedAt;
                    string url = article.Url;
                    if (title != null && author != null && desp != null && urltoimg != null && content != null)
                    {
                        SqlCommand cmd;
                        try
                        {

                            all_news_count++;
                            cmd = new SqlCommand("INSERT INTO dbo.News (NewsID,Category) VALUES (@n,@c)", conn);
                            cmd.Parameters.Add("@n", all_news_count);
                            cmd.Parameters.Add("@c", "General");

                            conn.Open();
                            x = cmd.ExecuteNonQuery();
                            if (x > 0)
                            {
                                Console.WriteLine("success");
                            }
                            else
                            {
                                Console.WriteLine("fail");
                            }


                           
                            general_id++;
                            cmd = new SqlCommand("INSERT INTO dbo.General (NewsID,Title,Author,Desp,urlToImg,Content,PublishedAt,NiD,url) VALUES (@n,@t,@a,@d,@u,@c,@p,@fk,@ur)", conn);
                            cmd.Parameters.Add("@n", general_id);
                            cmd.Parameters.Add("@t", title);
                            cmd.Parameters.Add("@a", author);
                            cmd.Parameters.Add("@d", desp);
                            cmd.Parameters.Add("@u", urltoimg);
                            if (content.Length < 260)
                            {
                                cmd.Parameters.Add("@c", content.Substring(0, content.Length));
                            }
                            else
                            {
                                cmd.Parameters.Add("@c", content.Substring(0, 260));

                            }
                            cmd.Parameters.Add("@p", publishedAt);
                            cmd.Parameters.Add("@fk", all_news_count);
                            cmd.Parameters.Add("@ur", url);
                         
                            x = cmd.ExecuteNonQuery();
                            if (x > 0)
                            {
                                File.AppendAllText($"{service_location}\\Log_Debug.txt", "General Table Filled" + $"{System.Environment.NewLine}");
                            }
                            else
                            {
                                File.AppendAllText($"{service_location}\\Log_Debug.txt", "General Table Filled failed" + $"{System.Environment.NewLine}");
                            }

                            

                        }
                        catch (Exception ex)
                        {
                            general_id--;
                            all_news_count--;
                            File.AppendAllText($"{service_location}\\Log_Debug.txt", "Query failure : "+ ex.Message + $"{System.Environment.NewLine}");
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }

            }

            }

        protected override void OnStop()
        {
            File.AppendAllText($"{service_location}\\Log_Debug.txt", "OnStop"  + $"{System.Environment.NewLine}");

        }
    }
}
