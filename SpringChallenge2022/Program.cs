using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    public partial class Entity
    {
        public int Id { get; set; }
        public EntityType Type { get; set; }
        public Coord Coord { get; set; }

        public int ShieldLife { get; set; }
        public int IsControlled { get; set; }
        public int Health { get; set; }
        public Coord VCorrd { get; set; }
        public NearBaseType NearBase { get; set; }
        public ThreatType ThreatFor { get; set; }

        public Entity(int id, EntityType type, Coord coord, int shieldLife, int isControlled, int health, Coord vCorrd, NearBaseType nearBase, ThreatType threatFor)
        {
            Id = id;
            Coord = coord;
            Type = type;
            ShieldLife = shieldLife;
            IsControlled = isControlled;
            Health = health;
            VCorrd = vCorrd;
            NearBase = nearBase;
            ThreatFor = threatFor;
        }
    }

    public class Coord
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsActive { get; set; }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Monster : Entity
    {
        public bool IsMoveForwardBase { get; set; }
        public int MaxTurnToKill { get; set; }
        public Monster(int id, EntityType type, Coord coord, int shieldLife, int isControlled, int health, Coord vCoord, NearBaseType nearBase, ThreatType threatFor) : base(id, type, coord, shieldLife, isControlled, health, vCoord, nearBase, threatFor)
        {
            IsMoveForwardBase = CheckMonsterGoingInMyBase();
            MaxTurnToKill = GetTurnToKill(); // Need to implement distance of hero to kill them
        }

        public bool CheckMonsterGoingInMyBase()
        {
            Coord nextPosition = new Coord(Coord.X, Coord.Y);
            for (int i = 0; i < 10; i++)
            {
                // Check if next position is in my base
                if (IsInMyBase(nextPosition))
                {
                    Console.Error.WriteLine("Monster is going in my base");
                    return true;
                }
                nextPosition = new Coord(Coord.X + VCorrd.X, Coord.Y + VCorrd.Y);
            }
            return false;
        }
        public int GetTurnToKill()
        {
            int monsterHealth = Health;
            int turnToKill = 0;
            bool monsterIsDead = false;
            Coord nextPosition = new Coord(Coord.X, Coord.Y);
            // Check if next position is in my base or monster is dead
            while (!IsKillingMyBase(nextPosition) || monsterIsDead)
            {
                turnToKill++;
                monsterHealth -= 2;
                if (monsterHealth <= 0)
                {
                    monsterIsDead = true;
                }
                nextPosition = new Coord(nextPosition.X + VCorrd.X, nextPosition.Y + VCorrd.Y);
            }
            return turnToKill;
        }
        public Monster FindMonterControl()
        {
            return null;
        }
        public bool IsTarget(List<Target> targets)
        {
            foreach (var target in targets)
            {
                if (target.MonterId == Id)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsInRange(Entity entity)
        {
            return CalculDistance(Coord, entity.Coord) <= 2200;
        }
        public bool IsTargetBase()
        {
            return NearBase == NearBaseType.TARGET_BASE;
        }
        public bool IsThreatBase()
        {
            return ThreatFor == ThreatType.HERO_BASE;
        }
        public bool IsNotInMyBase(Coord myBaseCoord) => CalculDistance(Coord, myBaseCoord) > 5000;
        public bool IsInMyBase(Coord myBaseCoord) => CalculDistance(Coord, myBaseCoord) <= 5000;
        public bool IsKillingMyBase(Coord myBaseCoord) => CalculDistance(Coord, myBaseCoord) < 200;
        public bool IsOnlyFrontOfBase(Coord myBaseCoord) => CalculDistance(Coord, myBaseCoord) > 5000 && CalculDistance(Coord, myBaseCoord) <= 7000;
        public bool IsInRangeSpell(Spell spell, Coord coord)
        {
            var isInRange = false;
            switch (spell)
            {
                case Spell.CONTROL: isInRange = CalculDistance(Coord, coord) <= 2200; break;
                case Spell.SHIELD: isInRange = CalculDistance(Coord, coord) <= 2200; break;
                case Spell.WIND: isInRange = CalculDistance(Coord, coord) <= 1280; break;
            }
            return isInRange;
        }
    }

    public class Hero : Entity
    {
        public bool IsControlledSoon { get; set; }
        public Monster? Target { get; set; }
        public Coord CloseLockPoint { get; set; }
        public Area LockPoints { get; set; }
        public Hero(int id, EntityType type, Coord coord, int shieldLife, int isControlled, int health, Coord vCoord, NearBaseType nearBase, ThreatType threatFor) : base(id, type, coord, shieldLife, isControlled, health, vCoord, nearBase, threatFor)
        {
            Target = null;
            IsControlledSoon = false;
        }

        public void Update(Coord coord, int shieldLife, int isControlled, int health, Coord vCoord, NearBaseType nearBase, ThreatType threatFor, Field field)
        {
            Coord = coord;
            ShieldLife = shieldLife;
            IsControlled = isControlled;
            Health = health;
            VCorrd = vCoord;
            NearBase = nearBase;
            ThreatFor = threatFor;
            var newlock = field.MyLockPoints.Where(l => l.IsActive).OrderBy(l => CalculDistance(Coord, l));
            CloseLockPoint = newlock.Any() ? newlock.First() : new Coord(0, 0);
            CloseLockPoint.IsActive = false;
            UpdateLockPoints(field);

        }
        public void UpdateLockPoints(Field field)
        {
            var locks = field.Areas.Where(a => !a.IsTake).OrderBy(a => CalculDistance(Coord, a.AverageCoord()));
            if (CalculDistance(Coord, field.MyBase) <= 5000 && locks.Any())
            {
                var lockFind = locks.First();
                LockPoints = lockFind;
                lockFind.IsTake = true;
            }
        }

        public void CheckArea(Field field)
        {
            if (CalculDistance(Coord, field.MyBase) < 5000)
            {
                field.Areas.Find(a => a.Id == LockPoints.Id).IsTake = false;
            }
        }
        public void MoveToLockPoints()
        {
            if (!LockPoints.Points.Where(p => !p.IsVisited).ToList().Any())
            {
                LockPoints.Reset();
            }
            var point = LockPoints.Points.Where(p => !p.IsVisited).First();
            Console.Error.WriteLine(point.Coord);
            Console.Error.WriteLine( $"{Id}");
            LockPoints.Points.ForEach(l => {
                Console.Error.WriteLine($"{l.Coord.X} {l.Coord.Y}");
            });
            point.IsVisited = true;
            Move(point.Coord);
        }

        public void UpdateTarget(List<Monster> monsters)
        {
            if (monsters.Any() && Target != null && monsters.Contains(Target))
            {
                Target = monsters.First(m => m.Id == Target.Id);
            }
            else
            {
                Target = null;
            }
        }
        public void Wait()
        {
            Console.WriteLine("WAIT");
        }
        public void Move(Coord coord)
        {
            Console.WriteLine($"MOVE {coord.X} {coord.Y} ");
        }
        public void CastSpell(Spell spellType, int targetId, Coord targetedCoord)
        {
            switch (spellType)
            {
                case Spell.WIND:
                    Console.WriteLine($"SPELL WIND {targetedCoord.X} {targetedCoord.Y}");
                    break;
                case Spell.SHIELD:
                    Console.WriteLine($"SPELL SHIELD {targetId}");
                    break;
                case Spell.CONTROL:
                    Console.WriteLine($"SPELL CONTROL {targetId} {targetedCoord.X} {targetedCoord.Y}");
                    break;
            }
        }
        public bool CanWindMonster(Monster monster, Field field)
        {
            if (field.MyMana > 10 && CalculDistance(Coord, monster.Coord) <= 1280 && monster.ShieldLife == 0)
            {
                if (CalculDistance(field.MyBase, monster.Coord) < 3000) return true;

                if (monster.Health > 12 && monster.IsMoveForwardBase && monster.IsInMyBase(field.MyBase) && CalculDistance(field.MyBase, monster.Coord) < 6000) return true;

                if (monster.Health > 11 && monster.IsMoveForwardBase && monster.NearBase == NearBaseType.TARGET_BASE && CalculDistance(field.MyBase, monster.Coord) < 6000) return true;

            }
            return false;
        }
        public bool CanControlledMonter(Monster monster)
        {
            return CalculDistance(Coord, monster.Coord) < 2200;
        }
        public void Attack(List<Monster> monsters, Field field)
        {
            var monstersAround = monsters.Where(m => m.IsInRange(this)).OrderBy(m => CalculDistance(Coord, m.Coord)).ToList();
            Console.Error.WriteLine($"{IsControlled}");
            if (IsControlled > 0)
            {
                IsControlledSoon = true;
            }
            if (CalculDistance(Coord, field.EnemyBase) < 6000 && IsControlledSoon && ShieldLife == 0)
            {
                CastSpell(Spell.SHIELD, Id, null);
                IsControlledSoon = false;
            } else
            {
                if (CalculDistance(Coord, field.EnemyBase) > 5500)
                {

                    // move around lockPoint Enemy 
                    var FirstLockPoint = field.EnemyLockPoints.OrderBy(l => CalculDistance(Coord, l)).First();
                    var SecondLockPoint = field.EnemyLockPoints.OrderBy(l => CalculDistance(Coord, l)).Skip(1).First();
                    if (CalculDistance(Coord, FirstLockPoint) > CalculDistance(Coord, SecondLockPoint) && (CalculDistance(Coord, FirstLockPoint) - CalculDistance(Coord, SecondLockPoint)) > 100)
                    {
                        Move(SecondLockPoint);
                    }
                    else
                    {
                        Move(FirstLockPoint);
                    }
                }
                else
                {
                    if (monstersAround.Any() && field.MyMana > 80)
                    {
                        var monsterShield = monstersAround.Where(m => m.IsInRangeSpell(Spell.SHIELD, m.Coord) && m.ShieldLife < 2).OrderBy(m => CalculDistance(Coord, m.Coord)).FirstOrDefault();
                        var monsterWind = monstersAround.Where(m => m.IsInRangeSpell(Spell.WIND, m.Coord) && m.ShieldLife == 0 && CalculDistance(Coord, field.EnemyBase) < 5000).OrderBy(m => CalculDistance(Coord, m.Coord)).FirstOrDefault();
                        var monsterControl = monstersAround.Where(m => m.IsInRangeSpell(Spell.CONTROL, m.Coord) && m.ShieldLife == 0 && CalculDistance(Coord, field.EnemyBase) < 5000).OrderBy(m => CalculDistance(Coord, m.Coord)).FirstOrDefault();
                        var monster = monstersAround.Where(m => m.ShieldLife == 0);
                        if (monsterShield != null)
                        {
                            CastSpell(Spell.SHIELD, monsterShield.Id, field.EnemyBase);
                        }
                        else if (monsterWind != null)
                        {
                            CastSpell(Spell.WIND, monsterWind.Id, field.EnemyBase);
                        }
                        else if (monsterControl != null)
                        {
                            CastSpell(Spell.CONTROL, monsterControl.Id, field.EnemyBase);
                        }
                        else
                        {
                            if (monster.Any())
                            {
                                Move(monster.First().Coord);
                            } else
                            {
                                var FirstLockPoint = field.EnemyLockPoints.OrderBy(l => CalculDistance(Coord, l)).First();
                                var SecondLockPoint = field.EnemyLockPoints.OrderBy(l => CalculDistance(Coord, l)).Skip(1).First();
                                if (CalculDistance(Coord, FirstLockPoint) > CalculDistance(Coord, SecondLockPoint) && (CalculDistance(Coord, FirstLockPoint) - CalculDistance(Coord, SecondLockPoint)) > 100)
                                {
                                    Move(SecondLockPoint);
                                }
                                else
                                {
                                    Move(FirstLockPoint);
                                }
                            }
                        }

                    }
                    else
                    {
                        // move around lockPoint Enemy 
                        var FirstLockPoint = field.EnemyLockPoints.OrderBy(l => CalculDistance(Coord, l)).First();
                        var SecondLockPoint = field.EnemyLockPoints.OrderBy(l => CalculDistance(Coord, l)).Skip(1).First();
                        if (CalculDistance(Coord, FirstLockPoint) > CalculDistance(Coord, SecondLockPoint) && (CalculDistance(Coord, FirstLockPoint) - CalculDistance(Coord, SecondLockPoint)) > 100)
                        {
                            Move(SecondLockPoint);
                        }
                        else
                        {
                            Move(FirstLockPoint);
                        }
                    }
                }
            }
        }
        public void SearchMonsterAlone(List<Monster> monsters, Field field)
        {


        }
        public void CalculCloseLockPoint(Field field)
        {
            var newlock = field.MyLockPoints.Where(l => l.IsActive).OrderBy(l => CalculDistance(Coord, l));
            if (newlock.Any())
            {
                CloseLockPoint = newlock.First();
                field.MyLockPoints.ForEach(l =>
                {
                    if (l == newlock)
                    {
                        l.IsActive = false;
                    }
                });
            }
        }
        public List<Monster> FindDangerousMonsters(List<Monster> monsters, Field field)
        {
            return monsters.Where(m => m.IsMoveForwardBase || m.IsTargetBase() || m.IsThreatBase()).OrderBy(m => CalculDistance(m.Coord, field.MyBase)).ToList();
        }
        public Coord CoordMonsterClose(List<Monster> monsters, Field field)
        {
            var monster = monsters.Where(m => m.IsInRange(this)).OrderBy(m => CalculDistance(m.Coord, field.MyBase)).First();
            return monster.Coord;
        }
    }

    public class Area
    {
        public int Id { get; set; }
        public List<Point> Points {get; set;}
        public bool IsTake { get; set; }

        public Area(int id)
        {
            Id = id;
            Points = new List<Point>();
            IsTake = false;
        }
        public void Reset()
        {
            Points.ForEach(p => p.IsVisited = false);
        }

        public Coord AverageCoord()
        {
            return new Coord((int)Points.Average(a => a.Coord.X), (int)Points.Average(a => a.Coord.Y));
        }
    }
    

    public class Point
    {
        public int Id { get; set; }
        public Coord Coord { get; set; }
        public bool IsVisited { get; set; }

        public Point(int id, Coord coord)
        {
            Id = id;
            Coord = coord;
            IsVisited = false;
        }
    }
    
    public class Field
    {
        public const int MAX_BASE = 5000;
        public int BASE_TILE = MAX_BASE / 8;
        const int MaxMapX = 17630;
        const int MaxMapY = 9000;

        public int Round { get; set; }
        public Coord CenterPoint { get; set; }
        public Coord MyBase { get; set; }
        public Coord EnemyBase { get; set; }
        public List<Coord> EnemyLockPoints { get; set; }
        public List<Coord> MyLockPoints { get; set; }
        public List<Area> Areas { get; set; }
        public int MyMana { get; set; }
        public int MyHealth { get; set; }
        public int EnemyHealth { get; set; }
        public int EnemyMana { get; set; }

        public Field(Coord myBase)
        {
            MyBase = myBase;
            EnemyBase = new Coord(MaxMapX - myBase.X, MaxMapY - myBase.Y); // The corner of the map representing your opponent's base
            CenterPoint = new Coord(MaxMapX / 2, MaxMapY / 2);
            MyLockPoints = new List<Coord>();
            EnemyLockPoints = new List<Coord>();
            Areas = new List<Area>();
            CalculLockPoints();
            Round = 0;
        }

        public void CalculLockPoints()
        {
            MyLockPoints.Add(new Coord(MyBase.X - BASE_TILE * 10, MyBase.Y - BASE_TILE * 10));
            MyLockPoints.Add(new Coord(MyBase.X - BASE_TILE * 10, MyBase.Y - BASE_TILE * 6));
            MyLockPoints.Add(new Coord(MyBase.X - BASE_TILE * 12, MyBase.Y - BASE_TILE * 4));

            EnemyLockPoints.Add(new Coord(EnemyBase.X - BASE_TILE * 3, EnemyBase.Y - BASE_TILE * 7));
            EnemyLockPoints.Add(new Coord(EnemyBase.X - BASE_TILE * 7, EnemyBase.Y - BASE_TILE * 3));

            var AreaNorth = new Area(0);
            var AreaMid = new Area(1);
            var AreaSouth = new Area(2);

            AreaNorth.Points.Add(new Point(0,new Coord(MyBase.X - MAX_BASE, MyBase.Y)));
            AreaNorth.Points.Add(new Point(1,new Coord(MyBase.X - (MAX_BASE * 2) + MAX_BASE/4, MyBase.Y)));
            AreaNorth.Points.Add(new Point(2, new Coord(MyBase.X - MAX_BASE - BASE_TILE, MyBase.Y - (BASE_TILE * 2))));
            AreaNorth.Points.Add(new Point(3,new Coord(MyBase.X - (MAX_BASE * 2) - BASE_TILE + MAX_BASE/4, MyBase.Y - (BASE_TILE * 3))));

            AreaSouth.Points.Add(new Point(0, new Coord(MyBase.X - MAX_BASE - BASE_TILE, MyBase.Y - (BASE_TILE * 2))));
            AreaSouth.Points.Add(new Point(1, new Coord(MyBase.X - (MAX_BASE * 2) - BASE_TILE + MAX_BASE / 4, MyBase.Y - (BASE_TILE * 3))));
            AreaSouth.Points.Add(new Point(2, new Coord(MyBase.X - (MAX_BASE * 2) + MAX_BASE / 4, MyBase.Y - (BASE_TILE * 2))));
            AreaSouth.Points.Add(new Point(3, new Coord(MyBase.X - (MAX_BASE * 2) + MAX_BASE / 4, MyBase.Y - (BASE_TILE * 3))));

            AreaMid.Points.Add(new Point(0, new Coord(MyBase.X - (MAX_BASE * 2) + MAX_BASE / 4, MyBase.Y - (BASE_TILE * 2))));
            AreaMid.Points.Add(new Point(1, new Coord(MyBase.X - (MAX_BASE * 2) + MAX_BASE / 4, MyBase.Y - (BASE_TILE * 3))));
            AreaMid.Points.Add(new Point(2, new Coord(MyBase.X - (MAX_BASE * 2) + MAX_BASE / 4, MyBase.Y - (BASE_TILE * 4))));
            AreaMid.Points.Add(new Point(3, new Coord(MyBase.X - (MAX_BASE * 2) + MAX_BASE / 4, MyBase.Y - (BASE_TILE * 5))));

            // Check value are positif
            AreaNorth.Points.ForEach(a => a.Coord.X = Math.Abs(a.Coord.X));
            AreaNorth.Points.ForEach(a => a.Coord.Y = Math.Abs(a.Coord.Y));
            AreaMid.Points.ForEach(a => a.Coord.X = Math.Abs(a.Coord.X));
            AreaMid.Points.ForEach(a => a.Coord.Y = Math.Abs(a.Coord.Y));
            AreaSouth.Points.ForEach(a => a.Coord.X = Math.Abs(a.Coord.X));
            AreaSouth.Points.ForEach(a => a.Coord.Y = Math.Abs(a.Coord.Y));

            Areas.Add(AreaNorth);
            Areas.Add(AreaMid);
            Areas.Add(AreaSouth);

            // Check value are positif
            MyLockPoints.ForEach(coord =>
            {
                if (coord.X < 0) coord.X = coord.X * -1;
                if (coord.Y < 0) coord.Y = coord.Y * -1;
            });

            EnemyLockPoints.ForEach(coord =>
            {
                if (coord.X < 0) coord.X = coord.X * -1;
                if (coord.Y < 0) coord.Y = coord.Y * -1;
            });
        }
    }
    public enum EntityType
    {
        HERO = 0,
        MONSTER = 1,
        OPPONENTS = 2,
    }
    public enum NearBaseType
    {// 0=monster with no target yet, 1=monster targeting a base
        NO_TARGET = 0,
        TARGET_BASE = 1,
    }
    public enum ThreatType
    {// Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither
        HERO_BASE = 1,
        OPPONENTS_BASE = 2,
        NEITHER = 0,
    }
    public enum Spell
    {
        CONTROL = 0,
        SHIELD = 1,
        WIND = 2,
    }
    public class Target
    {
        public int MonterId { get; set; }
        public int TargetedBy { get; set; }

        public Target(int id, int targetedBy)
        {
            MonterId = id;
            TargetedBy = targetedBy;
        }
    }
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');

        Coord myBase = new Coord(int.Parse(inputs[0]), int.Parse(inputs[1])); // The corner of the map representing your base
        int heroesPerPlayer = int.Parse(Console.ReadLine()); // Always 3
        Field field = new Field(myBase);
        List<Monster> montersSpelled = new List<Monster>();
        List<Hero> heros = new List<Hero>();

        field.Areas.ForEach(a => a.IsTake = false);
        // game loop
        while (true)
        {
            List<Monster> monsters = new List<Monster>();
            List<Entity> entities = new List<Entity>();
            List<Monster> monstersTarget = new List<Monster>();
            List<Entity> opponents = new List<Entity>();

            inputs = Console.ReadLine().Split(' ');
            field.MyHealth = int.Parse(inputs[0]); 
            field.MyMana = int.Parse(inputs[1]);

            inputs = Console.ReadLine().Split(' ');
            field.EnemyHealth = int.Parse(inputs[0]); 
            field.EnemyMana = int.Parse(inputs[1]); 
           
            field.Round++;
            
            // Reset LockPoints
            field.MyLockPoints.ForEach(p => p.IsActive = true);
            


            int entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]); // Unique identifier
                int type = int.Parse(inputs[1]); // 0=monster, 1=your hero, 2=opponent hero
                int x = int.Parse(inputs[2]); // Position of this entity
                int y = int.Parse(inputs[3]);
                int shieldLife = int.Parse(inputs[4]); // Ignore for this league; Count down until shield spell fades
                int isControlled = int.Parse(inputs[5]); // Ignore for this league; Equals 1 when this entity is under a control spell
                int health = int.Parse(inputs[6]); // Remaining health of this monster
                int vx = int.Parse(inputs[7]); // Trajectory of this monster
                int vy = int.Parse(inputs[8]);
                int nearBase = int.Parse(inputs[9]); // 0=monster with no target yet, 1=monster targeting a base
                int threatFor = int.Parse(inputs[10]); // Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither
                var entity = new Entity(id, (EntityType)type, new Coord(x, y), shieldLife, isControlled, health, new Coord(vx, vy), (NearBaseType)nearBase, (ThreatType)threatFor);
                if (type == 0)
                {
                    monsters.Add(new Monster(id, (EntityType)type, new Coord(x, y), shieldLife, isControlled, health, new Coord(vx, vy), (NearBaseType)nearBase, (ThreatType)threatFor));
                }
                else if (type == 1)
                {
                    var hero = heros.FirstOrDefault(h => h.Id == entity.Id);
                    if (hero == null)
                    {
                        var heroAdd = new Hero(id, (EntityType)type, new Coord(x, y), shieldLife, isControlled, health, new Coord(vx, vy), (NearBaseType)nearBase, (ThreatType)threatFor);
                        heros.Add(heroAdd);
                        heroAdd.Update(new Coord(x, y), shieldLife, isControlled, health, new Coord(vx, vy), (NearBaseType)nearBase, (ThreatType)threatFor, field);
                        field.Areas.ForEach(a =>
                        {
                            Console.Error.WriteLine($"{a.Id} {a.Points.Count} {a.IsTake}");
                        });
                        heroAdd.LockPoints = field.Areas.Where(a => !a.IsTake).First();
                        heroAdd.LockPoints.Points.ForEach(l => l.IsVisited = false);
                        field.Areas.Find(a => a.Id == heroAdd.LockPoints.Id).IsTake = true;
                    }
                    else
                    {
                        hero.Update(new Coord(x, y), shieldLife, isControlled, health, new Coord(vx, vy), (NearBaseType)nearBase, (ThreatType)threatFor, field);
                    }
                }
                else
                {
                    opponents.Add(entity);
                }
            }
                
            // TOODO

            // GAMING 
            for (int cpt = 0; cpt < heros.Count; cpt++)
            {
                var hero = heros.ElementAt(cpt);
                // hero.UpdateTarget(monsters);

                if (field.Round > 80 && cpt == 0)
                {
                    hero.Attack(monsters, field);
                }
                else
                {
                    var opClose = opponents.Where(o => CalculDistance(o.Coord, field.MyBase) < 9000).OrderBy(o => CalculDistance(o.Coord, hero.Coord)).ToList();
                    var dangerousMonsters = hero.FindDangerousMonsters(monsters, field);
                    if(dangerousMonsters.Any() && CalculDistance(dangerousMonsters.First().Coord, field.MyBase) < 3000 && opClose.Any() && hero.ShieldLife == 0)
                    {
                        hero.CastSpell(Spell.SHIELD, hero.Id, null);
                    }
                    else  if (monsters.Any())
                    {
                        if (dangerousMonsters.Any())
                        {
                            var monsterTarget = dangerousMonsters.First();

                            if (monstersTarget != null && hero.CanWindMonster(monsterTarget, field) && !montersSpelled.Contains(monsterTarget))
                            {
                                hero.CastSpell(Spell.WIND, monsterTarget.Id, field.CenterPoint);
                                // montersSpelled.Add(monsterTarget);
                                field.MyMana -= 10;
                            }
                            else if (!montersSpelled.Contains(monsterTarget) && field.MyMana > 10 && monsterTarget.ShieldLife == 0 && monsterTarget.Health > 15 && monsterTarget.IsNotInMyBase(field.MyBase) && monsterTarget.IsMoveForwardBase && hero.CanControlledMonter(monsterTarget))
                            {
                                hero.CastSpell(Spell.CONTROL, monsterTarget.Id, field.CenterPoint);
                                field.MyMana -= 10;
                                montersSpelled.Add(monsterTarget);
                            }
                            else
                            {
                                hero.Move(monsterTarget.Coord);
                            }
                        }
                        else
                        {

                            var ManaFarmMonsters = monsters.Where(m => m.IsOnlyFrontOfBase(m.Coord) && CalculDistance(m.Coord, hero.Coord) < 3000).OrderBy(m => CalculDistance(m.Coord, hero.Coord)).ToList();

                            if (ManaFarmMonsters.Any())
                            {
                                var monsterTarget = ManaFarmMonsters.First();
                                if (!montersSpelled.Contains(monsterTarget) && field.MyMana > 10 && monsterTarget.ShieldLife == 0 && monsterTarget.Health > 15 && monsterTarget.IsNotInMyBase(field.MyBase) && monsterTarget.IsMoveForwardBase && hero.CanControlledMonter(monsterTarget))
                                {
                                    hero.CastSpell(Spell.CONTROL, monsterTarget.Id, field.CenterPoint);
                                    field.MyMana -= 10;
                                    montersSpelled.Add(monsterTarget);
                                }
                                else
                                {
                                    hero.Move(monsterTarget.Coord);
                                }
                            }
                            else
                            {
                                hero.MoveToLockPoints();
                                //hero.Move(hero.LockPoints.AverageCoord());
                            }
                        }
                    }
                    else
                    {
                        hero.MoveToLockPoints();
                        // hero.Move(hero.LockPoints.AverageCoord());
                        
                    }
                }

                hero.CheckArea(field);
            }
        }
    }

    public static int CalculDistance(Coord c1, Coord c2)
    {
        return (int)Math.Sqrt(Math.Pow(c1.X - c2.X, 2) + Math.Pow(c1.Y - c2.Y, 2));
    }

}

