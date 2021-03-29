using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace ScoreModelImplement
{
    public class ScoreModel
    {
        /// <summary>
        /// Обратная польская запись для интегрированной оценки
        /// </summary>
        private string ReversePolishResult;

        /// <summary>
        /// Варинт выбора и его стоимость в оценке
        /// </summary>
        private struct Choice
        {

            public Choice(string name, float value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }

            public float Value { get; }

            public override string ToString() => $"{Name} ({Value})";
        }

        /// <summary>
        /// Имя правила и варианты выбора
        /// </summary>
        private struct Rule
        {
            public Rule(string name, float weight, Choice[] ruleChoices)
            {
                Name = name;
                Weight = weight;
                RuleChoices = ruleChoices;
            }

            public string Name { get; }

            public float Weight { get; }

            public Choice[] RuleChoices { get; }

            public override string ToString() => $"{Name} ({Weight}) [{RuleChoices.Length}]";
        }

        /// <summary>
        /// Вся модель
        /// </summary>
        private Rule[] Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">Правила в формате: *Имя правила1*!*Вес*:*Имя Варианта1*_*Стоимость1*!*Имя Варианта2*_*Стоимость2* ... \n</param>
        public ScoreModel(string data)
        {
            ReversePolishResult = null;
            //Делим на строки
            string[] rules_choices = data.Split('\n');
            //Делим на правила и варианты
            string[] rules = rules_choices.Select(s => s.Split(':')[0]).ToArray();
            string[] choices = rules_choices.Select(s => s.Split(':')[1]).ToArray();

            Data = new Rule[rules.Length];

            for (int i = 0; i < rules.Length; i++)
            {
                //Считываем варианты для правила
                string[] cho = choices[i].Split('!');
                int choicesN = choices[i].Split('!').Length;
                Choice[] actualChoices = new Choice[choicesN];
                for (int j = 0; j < choicesN; j++)
                {
                    string choiceName = cho[j].Split('_')[0];
                    float choiceValue = float.Parse(cho[j].Split('_')[1]);

                    actualChoices[j] = new Choice(choiceName, choiceValue);
                }

                string ruleName = rules[i].Split('!')[0];
                float ruleWeight = float.Parse(rules[i].Split('!')[1]);
                Data[i] = new Rule(ruleName, ruleWeight, actualChoices);
            }
        }

        /// <summary>
        /// Оценка по выбранным ответам
        /// </summary>
        /// <param name="choices"> Выбраные ответы </param>
        /// <returns> Пока суммарная оценка </returns>
        public float ReturnScore(int[] choices)
        {

            if (choices.Length != Data.Length)
                throw new Exception("Несоответсвие количества ответов количеству правил");

            float result = 0;

            if (ReversePolishResult == null)
                for (int i = 0; i < choices.Length; i++)
                    result += Data[i].Weight * Data[i].RuleChoices[choices[i]].Value;
            else
                CalculateWithPolish(choices);

            return result;
        }

        /// <summary>
        /// Возвращает смысл ответов
        /// </summary>
        /// <param name="choices"> Выбраные ответы </param>
        /// <returns>Вопросы и ответы клиента</returns>
        public string ReturnChoices(int[] choices)
        {
            string meaningChoices = "";

            for (int i = 0; i < choices.Length; i++)
                meaningChoices += Data[i].Name + ": " + Data[i].RuleChoices[choices[i]].Name + "\n";

            return meaningChoices;
        }

        /// <summary>
        /// Возвращает список правил в порядке их записи с нумерацией используемой в создании формулы
        /// </summary>
        /// <returns>Список правил для показа пользователю</returns>
        public string ReturnRules()
        {
            string result = "";
            for (int i = 0; i < Data.Length; i++)
                result += (i+1).ToString() + " - " + Data[i].Name + "\n";

            return result; 
        }

        /// <summary>
        /// Переводит строку в обратную польскую запись
        /// Используемые операторы: +,-,*,/,(,),^.
        /// Без унарных минусов.
        /// Правила в записи указываются по номеру в порядке инициализации класса.
        /// </summary>
        /// <param name="mathExpression">Математическое выражение для перевода в польскую запись</param>
        /// <returns>Математическое выражение в формате обратной польской записи. Разделитель ","</returns>
        public string ToReversePolishNotation(string mathExpression)
        {
            string result = "";


            //Стэк для хранения операторов
            Stack<char> operatorStack = new Stack<char>(20);

            //Последний использованный символ
            char lastChar = '_';

            //Приоритеты операций. Больше число - выше приоритет
            Dictionary<char, int> opPriority = new Dictionary<char, int>
            {
                ['('] = 0,
                [')'] = 0,
                ['+'] = 1,
                ['-'] = 1,
                ['*'] = 2,
                ['/'] = 2,
                ['^'] = 3
            };

            if (mathExpression.Count(c => c == '(') != mathExpression.Count(c => c == ')'))
                throw new Exception("Количество закрывающих скобок неравно количеству открывающих");

            for (int i = 0; i < mathExpression.Length; i++)
            {
                //Обработк цифр
                if (char.IsDigit(mathExpression[i]))
                {
                    switch (lastChar)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '_': break;

                        default: result += ","; break;
                    }
                    lastChar = mathExpression[i];
                    result += lastChar;
                }

                //Обработк операторов
                if (!char.IsDigit(mathExpression[i]))
                {
                    if (mathExpression[i] != ')' && mathExpression[i] != '(' && (lastChar == '-' || lastChar == '*' || lastChar == '/' || lastChar == '+' || lastChar == '^') 
                        && (lastChar != ')' || mathExpression[i] != '('))
                        throw new Exception("Ошибка в исходном выражении: два оператора подряд");

                    switch (mathExpression[i])
                    {

                        case '(': operatorStack.Push('('); break;
                        case ')':
                            {
                                if (!operatorStack.Contains('('))
                                    throw new Exception("Ошибка в исходном выражении: нарушена последовательность скобок");

                                while (operatorStack.Peek() != '(')
                                    result += "," + operatorStack.Pop();

                                operatorStack.Pop();
                                break;
                            }

                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case '^':
                            {
                                while (operatorStack.Count != 0 && opPriority[operatorStack.Peek()] >= opPriority[mathExpression[i]])
                                    result += "," + operatorStack.Pop();

                                operatorStack.Push(mathExpression[i]);
                                break;
                            }




                        default: throw new Exception("Ошибка в исходном выражении: использован символ не являющийся математическим оператором");
                    }
                    lastChar = mathExpression[i];
                }
            }

            while (operatorStack.Count != 0)
                result += "," + operatorStack.Pop();

            ReversePolishResult = result;
            return result;
        }


        /// <summary>
        /// На основе обратной польской записи вычисляет интегрированную оценку
        /// </summary>
        /// <param name="choices">Выбранные ответы</param>
        /// <returns>Интегрированная оценка</returns>
        public double CalculateWithPolish(int[] choices)
        {
            object[] polishData = ReversePolishResult.Split(',');
            Stack<double> tempCalc = new Stack<double>(20);

            int rule = -1; //Номер правила
            double a, b; //Временные переменные для вычисления внутри польской записи

            for (int i = 0; i < polishData.Length; i++)
                if (int.TryParse((string)polishData[i], out rule))
                    tempCalc.Push(Data[rule-1].Weight * Data[rule-1].RuleChoices[choices[rule-1]].Value);
                else
                {
                    b = tempCalc.Pop();
                    a = tempCalc.Pop();
                    switch ((string)polishData[i])
                    {
                        case "-": tempCalc.Push(a - b); break;
                        case "+": tempCalc.Push(a + b); break;
                        case "*": tempCalc.Push(a * b); break;
                        case "/": tempCalc.Push(a / b); break;
                        case "^": tempCalc.Push(Math.Pow(a, b)); break;
                    }
                }

            return tempCalc.Pop();
        }


        
    }
}
