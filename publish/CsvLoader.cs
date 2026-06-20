using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PortalWeb;

namespace PortalWeb
{
    // 📚 シラバスの各行を表現するデータ構造
    public class SyllabusLine
    {
        public string Type { get; set; } = string.Empty;       // 「評価基準」または「授業計画」
        public string ItemName { get; set; } = string.Empty;   // 「期末試験」や「第1回」など
        public string Content { get; set; } = string.Empty;    // 「約70%」や「イントロダクション」など
    }

    public class CsvLoader
    {
        private readonly HttpClient _httpClient;

        public CsvLoader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 📅 時間割データを読み込む
        public async Task<List<MyScheduleItem>> LoadMyScheduleFromCsvAsync(string requestUrl)
        {
            var list = new List<MyScheduleItem>();
            try
            {
                string fixedUrl = requestUrl.StartsWith("./") ? requestUrl : "./" + requestUrl.TrimStart('/');
                var csvText = await _httpClient.GetStringAsync(fixedUrl);
                var lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length >= 5)
                    {
                        int.TryParse(parts[1], out int period);
                        list.Add(new MyScheduleItem
                        {
                            DayOfWeek = parts[0],
                            Period = period,
                            SubjectName = parts[2],
                            TeacherName = parts[3],
                            RoomName = parts[4]
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        // 📚 ★追加：シラバスデータをCSVから非同期で読み込む
        public async Task<List<SyllabusLine>> LoadSyllabusFromCsvAsync(string semester, string subjectName)
        {
            var list = new List<SyllabusLine>();
            string fileUrl = $"data/{semester}/syllabus/{subjectName}.csv";
            try
            {
                var csvText = await _httpClient.GetStringAsync(fileUrl);
                var lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    var parts = lines[i].Split(',');
                    if (parts.Length >= 3)
                    {
                        list.Add(new SyllabusLine
                        {
                            Type = parts[0].Trim(),
                            ItemName = parts[1].Trim(),
                            Content = parts[2].Trim()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"シラバス取得エラー ({subjectName}): {ex.Message}");
            }
            return list;
        }

        // 🚪 空き教室状況を読み込む
        public async Task<List<string>> LoadAvailableRoomsFromCsvAsync(string semester, string currentDay, int currentPeriod)
        {
            var availableRooms = new List<string>();
            var allRooms = new List<string> { "演習室1", "演習室3", "演習室5", "演習室6", "端末室1", "端末室2" };

            foreach (var roomName in allRooms)
            {
                string fileUrl = $"./data/{semester}/rooms/{roomName}.csv";
                try
                {
                    var csvText = await _httpClient.GetStringAsync(fileUrl);
                    var lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                    bool isOccupied = false;
                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;

                        var parts = lines[i].Split(',');
                        if (parts.Length >= 6)
                        {
                            string day = parts[0].Trim();
                            if (day.Contains(currentDay) || currentDay.Contains(day))
                            {
                                string state = parts[currentPeriod].Trim();
                                if (state == "使用中")
                                {
                                    isOccupied = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!isOccupied)
                    {
                        availableRooms.Add(roomName);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return availableRooms;
        }
    }
}