using System.ComponentModel;
using System.ComponentModel.Design;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace _Unity_9기__Text_RPG__스파르타_던전__과제
{
    public class Player
    {
        public string Name;
        public int Level = 1;
        public string Job = "전사";
        public float BaseAtt = 10;
        public int BaseDef = 5;
        public int BaseHealth = 100;
        public int CurrentHealth;
        public int Money = 1500;
        public List<Equipment> Inventory = new List<Equipment>();
        public bool IsDead = false;
        private int dungeonClearCount = 0;

        public Player(string name)
        {
            Name = name;
            CurrentHealth = TotalHealth();
        }

        public int GetBonusAtt() => Inventory.Where(e => e.IsEquipped && e.EquipType == "공격력").Sum(e => e.EquipStat);
        public int GetBonusDef() => Inventory.Where(e => e.IsEquipped && e.EquipType == "방어력").Sum(e => e.EquipStat);
        public int GetBonusHealth() => Inventory.Where(e => e.IsEquipped && e.EquipType == "체력").Sum(e => e.EquipStat);

        public int TotalHealth() => BaseHealth + GetBonusHealth();
        public int TotalDef() => BaseDef + GetBonusDef();
        public float TotalAtt() => BaseAtt + GetBonusAtt();

        public void IncreaseDungeonClearCount()
        {
            dungeonClearCount++;
            int requireClears = Level;
            if (dungeonClearCount >= requireClears)
            {
                Level++;
                BaseAtt += 0.5f;
                BaseDef += 1;
                CurrentHealth = Math.Min(CurrentHealth, TotalHealth());
                dungeonClearCount = 0;
                Console.WriteLine($"\n 레벨업하셨습니다. 현재 레벨: Lv.{Level} | 공격력: {BaseAtt} | 방어력: {BaseDef}");
            }
        }


        public void ShowStatus()
        {
            Console.WriteLine($"Lv. {Level.ToString("D2")}");
            Console.WriteLine($"Chad ( {Job} )");
            Console.WriteLine($"공격력 : {BaseAtt}" + (GetBonusAtt() != 0 ? $" (+{GetBonusAtt()})" : ""));
            Console.WriteLine($"방어력 : {BaseDef}" + (GetBonusDef() != 0 ? $" (+{GetBonusDef()})" : ""));
            Console.WriteLine($"체 력 : {CurrentHealth} / {BaseHealth}" + (GetBonusHealth() != 0 ? $" (+{GetBonusHealth()})" : ""));
            Console.WriteLine($"Gold : {Money} G");
        }
    }

    public class Equipment
    {
        public int No;
        public string EquipName;
        public string EquipType;
        public int EquipStat;
        public string Description;
        public int Price;
        public bool IsEquipped = false;
        public bool IsSold = false;

        public Equipment(int no, string name, string type, int stat, string description, int price)
        {
            No = no;
            EquipName = name;
            EquipType = type;
            EquipStat = stat;
            Description = description;
            Price = price;
        }

        public void Print(bool withIndex = false, int index = 0)
        {
            string status = IsEquipped ? "[E]" : "";
            string prefix = withIndex ? $"-{index + 1}" : "- ";
            Console.WriteLine($"{prefix} {status}{EquipName} | {EquipType} +{EquipStat} | {Description}");
        }

        public void PrintInStore(bool withIndex = false, int index = 0)
        {
            string suffix = IsSold ? "구매완료" : $"{Price}G";
            string prefix = withIndex ? $"-{index + 1}" : "- ";
            Console.WriteLine($"{prefix} {EquipName} | {EquipType} +{EquipStat} | {Description} | {suffix}");
        }

    }

    public class Dungeon
    {
        public string DungeonName;
        public int RecommendedDef;
        public int BaseReward;
        public Dungeon(string dungeonName, int recommendedDef, int baseReward)
        {
            DungeonName = dungeonName;
            RecommendedDef = recommendedDef;
            BaseReward = baseReward;
        }
    }

    internal class Program
    {
        List<Equipment> storeEquipment = new List<Equipment>()
        {
            new Equipment(1, "수련자 갑옷", "방어력", 5, "수련에 도움을 주는 갑옷입니다.", 1000),
            new Equipment(2, "무쇠갑옷", "방어력", 9, "무쇠로 만들어져 튼튼한 갑옷입니다.", 1800),
            new Equipment(3, "스파르타의 갑옷", "방어력", 15, "스파르타의 전사들이 사용했다는 전설의 갑옷입니다.", 3500),
            new Equipment(4, "낡은 검", "공격력", 2, "쉽게 볼 수 있는 낡은 검 입니다.", 600),
            new Equipment(5, "청동 도끼", "공격력", 5, "어디선가 사용됐던거 같은 도끼입니다.", 1500),
            new Equipment(6, "스파르타의 창", "공격력", 7, "스파르타의 전사들이 사용했다는 전설의 창입니다.", 3000),
            new Equipment(7, "생명의 반지", "체력", 20, "생명력을 북돋는 마법 반지입니다.", 1800),
            new Equipment(8, "고대의 생명석", "체력", 40, "고대 문명의 생명 에너지가 깃든 신비한 장비입니다.", 3600),
            new Equipment(9, "수련자 튜닉", "방어력", 2, "수련에 최적화된 가벼운 갑옷입니다.", 800),
            new Equipment(10, "철벽 갑옷", "방어력", 5, "단단한 철로 제작되어 높은 방어력을 자랑합니다.", 2200),
            new Equipment(11, "사자의 방패", "방어력", 8, "전설 속 전사가 사용했던 방패. 용맹의 상징입니다.", 3800),
            new Equipment(12, "낡은 훈련용 검", "공격력", 4, "훈련용으로 쓰이던 검. 사용감은 있지만 여전히 날카롭습니다.", 1000),
            new Equipment(13, "강철 전투도끼", "공격력", 10, "실전에 사용되는 무겁고 강력한 도끼입니다.", 2800),
        };

        string input = "";
        bool isAction = false;
        Player? player;
        Dungeon? Dungeon;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
        }

        public void Start()
        {
            Console.WriteLine("플레이어의 이름을 알려주세요.");
            string name = Console.ReadLine() ?? "이름없음";

            player = new Player(name);

            while (true)
            {
                MainMenu();
            }
        }

        void MainMenu()
        {
            Console.WriteLine($"\n\n{player!.Name}님 텍스트 마을에 오신 것을 환영합니다.");
            Console.WriteLine("이곳에서 던전으로 들어가기 전 활동을 할 수 있습니다.\n");
            Console.WriteLine("1. 상태 보기");
            Console.WriteLine("2. 인벤토리");
            Console.WriteLine("3. 상점");
            Console.WriteLine("4. 던전입장");
            Console.WriteLine("5. 휴식하기");

            Console.WriteLine("\n원하시는 행동을 입력해주세요");

            while (true)
            {
                input = Console.ReadLine() ?? "";
                isAction = int.TryParse(input, out int actionNumber);

                if (!isAction)
                {
                    Console.WriteLine("잘못된 입력입니다.");
                    continue;
                }

                switch (actionNumber)
                {
                    case 1:
                        CharacterStatus();
                        return;
                    case 2:
                        Inventory();
                        return;
                    case 3:
                        Store();
                        return;
                    case 4:
                        DungeonMenu();
                        return;
                    case 5:
                        BreakTime();
                        return;
                    default:
                        Console.WriteLine("잘못된 입력입니다.");
                        break;
                }
            }
        }
        public void CharacterStatus()
        {
            Console.WriteLine("\n\n상태 보기 \n캐릭터의 정보가 표시됩니다.");
            Console.WriteLine("");
            player!.ShowStatus();
            Console.WriteLine("\n0. 나가기 \n\n원하시는 행동을 입력해주세요.");

            while (true)
            {
                input = Console.ReadLine() ?? "";
                isAction = int.TryParse(input, out int actionNumber);

                if (isAction && actionNumber == 0) return;
                else Console.WriteLine("잘못된 입력입니다.");
            }
        }
        public void Inventory()
        {
            Console.WriteLine("\n\n인벤토리 \n보유 중인 아이템을 관리할 수 있습니다. \n\n[아이템 목록]");

            if (player!.Inventory.Count != 0)
            {
                foreach (var item in player!.Inventory)
                {
                    item.Print();
                }
            }

            Console.WriteLine("\n1. 장착 관리 \n0. 나가기 \n\n원하시는 행동을 입력해주세요.");

            while (true)
            {
                input = Console.ReadLine() ?? "";
                isAction = int.TryParse(input, out int actionNumber);

                if (isAction && actionNumber == 0) return;
                else if (actionNumber == 1)
                {
                    EquipmentManagement();
                    return;
                }
                else Console.WriteLine("잘못된 입력입니다.");
            }
        }

        public void EquipmentManagement()
        {
            while (true)
            {
                Console.WriteLine("\n\n인벤토리 - 장착 관리 \n보유 중인 아이템을 관리할 수 있습니다. \n\n[아이템 목록]");

                for (int i = 0; i < player!.Inventory.Count; i++)
                {
                    player.Inventory[i].Print(true, i);
                }
                Console.WriteLine("\n0. 나가기 \n\n원하시는 행동을 입력해주세요.");

                input = Console.ReadLine() ?? "";
                isAction = int.TryParse(input, out int actionNumber);

                if (isAction && actionNumber == 0) return;
                else if (actionNumber >= 1 && actionNumber <= player.Inventory.Count)
                {
                    var selected = player.Inventory[actionNumber - 1];
                    if (!selected.IsEquipped)
                    {
                        foreach (var item in player.Inventory)
                        {
                            if (item.IsEquipped && item.EquipType == selected.EquipType)
                            {
                                item.IsEquipped = false;
                                Console.WriteLine($"{item.EquipName}을(를) 해제했습니다.");
                            }
                        }
                        selected.IsEquipped = true;
                        Console.WriteLine($"{selected.EquipName}을(를) 장착했습니다.");
                    }
                    else
                    {
                        selected.IsEquipped = false;
                        Console.WriteLine($"{selected.EquipName}을(를) 해제했습니다.");
                    }
                    continue;
                }
                else Console.WriteLine("잘못된 입력입니다.");
            }
        }

        public void Store()
        {
            Console.WriteLine($"\n\n상점 \n필요한 아이템을 얻을 수 있는 상점입니다 \n\n[보유 골드]\n{player!.Money} G \n\n[아이템 목록]");

            for (int i = 0; i < storeEquipment.Count; i++)
            {
                storeEquipment[i].PrintInStore(false, i);
            }

            Console.WriteLine("\n1. 아이템 구매 \n2. 아이템 판매 \n0. 나가기 \n\n원하시는 행동을 입력해주세요.");

            while (true)
            {
                input = Console.ReadLine() ?? "";
                isAction = int.TryParse(input, out int actionNumber);

                if (isAction && actionNumber == 0) return;
                else if (actionNumber == 1)
                {
                    BuyItem();
                    return;
                }
                else if (actionNumber == 2)
                {
                    SellItem();
                    return;
                }
                else Console.WriteLine("잘못된 입력입니다.");
            }
        }

        public void BuyItem()
        {
            while (true)
            {
                Console.WriteLine($"\n\n상점 \n필요한 아이템을 얻을 수 있는 상점입니다 \n\n[보유 골드]\n{player!.Money} G \n\n[아이템 목록]");

                for (int i = 0; i < storeEquipment.Count; i++)
                {
                    storeEquipment[i].PrintInStore(true, i);
                }

                Console.WriteLine("\n0. 나가기 \n\n원하시는 행동을 입력해주세요.");


                input = Console.ReadLine() ?? "";
                isAction = int.TryParse(input, out int actionNumber);

                if (isAction && actionNumber == 0) return;
                else if (actionNumber >= 1 && actionNumber <= storeEquipment.Count)
                {
                    var item = storeEquipment[actionNumber - 1];

                    if (item.IsSold) Console.WriteLine("이미 구매한 아이템입니다.");
                    else if (player.Money < item.Price) Console.WriteLine("Gold가 부족합니다.");
                    else
                    {
                        var copy = new Equipment(item.No, item.EquipName, item.EquipType, item.EquipStat, item.Description, item.Price);
                        copy.IsSold = true;

                        player.Money -= item.Price;
                        item.IsSold = true;
                        player.Inventory.Add(copy);
                        Console.WriteLine($"{item.EquipName}을(를) 구매했습니다!");
                        continue;
                    }
                }
                else Console.WriteLine("잘못된 입력입니다.");
            }
        }

        public void SellItem()
        {
            while (true)
            {
                Console.WriteLine($"\n\n상점 \n필요한 아이템을 얻을 수 있는 상점입니다 \n\n[보유 골드]\n{player!.Money} G \n\n[아이템 목록]");

                for (int i = 0; i < player.Inventory.Count; i++)
                {
                    player.Inventory[i].Print(true, i);
                }

                Console.WriteLine("\n0. 나가기 \n\n원하시는 행동을 입력해주세요.");

                input = Console.ReadLine() ?? "";
                isAction = int.TryParse(input, out int actionNumber);

                if (isAction && actionNumber == 0) return;
                else if (actionNumber >= 1 && actionNumber <= player.Inventory.Count)
                {
                    var item = player.Inventory[actionNumber - 1];
                    int sellPrice = (int)(item.Price * 0.85);
                    if (item.IsEquipped)
                    {
                        item.IsEquipped = false;
                        Console.WriteLine($"[{item.EquipName}] 장착 해제됨");
                    }
                    player.Money += sellPrice;
                    player.Inventory.RemoveAt(actionNumber - 1);
                    var storeItem = storeEquipment.FirstOrDefault(e => e.No == item.No);
                    if (storeItem != null)
                    {
                        storeItem.IsSold = false;
                    }
                    Console.WriteLine($"{item.EquipName}을(를) {sellPrice} G에 판매했습니다!");
                    continue;
                }
                else Console.WriteLine("잘못된 입력입니다.");
            }


        }

        public void DungeonMenu()
        {
            List<Dungeon> dungeon = new List<Dungeon>
            {
                new Dungeon("쉬운 던전", 5, 1000),
                new Dungeon("보통 던전", 5, 1000),
                new Dungeon("어려운 던전", 5, 1000),
            };

            while (true)
            {
                Console.WriteLine("\n\n던전 \n이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.");
                Console.WriteLine($"\n1. 쉬운 던전  | 방어력 5 이상 권상");
                Console.WriteLine($"\n2. 일반 던전  | 방어력 11 이상 권상");
                Console.WriteLine($"\n3. 어려운 던전  | 방어력 17 이상 권상");
                Console.WriteLine("\n0. 나가기 \n\n원하시는 행동을 입력해주세요.");

                input = Console.ReadLine() ?? "";
                isAction = int.TryParse(input, out int actionNumber);

                if (isAction && actionNumber == 0) return;
                else if (actionNumber >= 1 && actionNumber <= 3)
                {
                    Dungeon selected = dungeon[actionNumber - 1];
                    GoDungeon(player!, selected);
                }
                else Console.WriteLine("잘못된 입력입니다.");
            }
        }
        public void GoDungeon(Player player, Dungeon dungeon)
        {
            Random rand = new Random();

            if (dungeon.RecommendedDef > player.TotalDef())
            {
                if (rand.Next(100) < 40)
                {
                    int lostHp = player.CurrentHealth / 2;
                    player.CurrentHealth -= lostHp;
                    Console.WriteLine($"던전 수행 실패하셨습니다. 체력 {lostHp}이 감소했습니다.");
                    PlayerDeath();
                    return;
                }
            }
            else
            {
                int defDiff = player.TotalDef() - dungeon.RecommendedDef;
                int minLost = 20 - defDiff;
                int maxLost = 35 - defDiff;

                int randomLost = Math.Max(0, rand.Next(minLost, maxLost));
                player.CurrentHealth -= randomLost;

                float randomBonus = rand.Next((int)player.TotalAtt(), (int)player.TotalAtt() * 2);
                float percentBonus = randomBonus / 100.0f;
                int bonus = (int)(dungeon.BaseReward * percentBonus);
                int totalGold = dungeon.BaseReward + bonus;

                player.Money += totalGold;
                PlayerDeath();

                Console.WriteLine($"던전 수행 성공하셨습니다. \n체력 {randomLost}이 감소했습니다. \n Gold {totalGold} G 얻었습니다.");

                player.IncreaseDungeonClearCount();
            }
        }

        public void BreakTime()
        {
            Console.WriteLine($"\n\n휴식하기 \n500 G 를 내면 체력을 회북할 수 있습니다. (보유 골드 : {player!.Money} G)");
            Console.WriteLine("\n1. 휴식하기 \n0. 나가기 \n\n원하시는 행동을 입력해주세요.");

            while (true)
            {
                input = Console.ReadLine() ?? "";
                isAction = int.TryParse(input, out int actionNumber);

                if (isAction && actionNumber == 0) return;
                else if (actionNumber == 1 && player!.Money >= 500)
                {
                    player.Money -= 500;
                    player.CurrentHealth += 100;
                    player.CurrentHealth = Math.Min(player.CurrentHealth, player.TotalHealth());
                    Console.WriteLine("휴식을 완료했습니다.");
                    return;
                }
                else if (actionNumber == 1 && player.Money < 500) Console.WriteLine("Gold가 부족합니다.");
                else Console.WriteLine("잘못된 입력입니다.");
            }
        }
        public void PlayerDeath()
        {
            if (player!.CurrentHealth <= 0)
            {
                player.IsDead = true;
                Console.WriteLine($"{player.CurrentHealth} / {player.TotalHealth()}가되어 캐릭터가 사망하셨습니다.");
                Environment.Exit(0);
            }
        }
    }
}