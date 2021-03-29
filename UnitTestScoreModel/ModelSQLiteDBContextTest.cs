using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using ScoreModelImplement;
using System.Configuration;


namespace UnitTestScoreModel
{
    [TestClass]
    public class ModelSQLiteDBContextTest
    {
        [TestMethod]
        public void Valid_AddПользователь()
        {
            //TODO: mock для тестирования контекста БД
            // Arrange
            DriverScoring.DBModels.mainEntitiesDB contextDB = new DriverScoring.DBModels.mainEntitiesDB();
         

            DriverScoring.DBModels.Пользователи person = new DriverScoring.DBModels.Пользователи();
            person.Login = "LOGIN_TEST";
            person.Password = "PASSWORD_TEST";

            // Act
            contextDB.Пользователи.Add(person);
            contextDB.SaveChanges();

            // Assert
            Assert.AreSame(person.Login, contextDB.Пользователи.Find(1).Login, "Не добавлен пользователь");
            Assert.AreSame(person.Password, contextDB.Пользователи.Find(1).Password, "Не добавлен пользователь");

            contextDB.Пользователи.Remove(person);
        }
    }
}
