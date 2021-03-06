﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace LiteDB.Tests.Document
{
    [TestClass]
    public class Bson_Tests
    {
        private BsonDocument CreateDoc()
        {
            // create same object, but using BsonDocument
            var doc = new BsonDocument();
            doc["_id"] = 123;
            doc["Address"] = new BsonDocument { ["city"] = "Atlanta", ["state"] = "XY" };
            doc["FirstString"] = "BEGIN this string \" has \" \t and this \f \n\r END";
            doc["CustomerId"] = Guid.NewGuid();
            doc["Phone"] = new BsonDocument { ["Mobile"] = "999", ["LandLine"] = "777" };
            doc["Date"] = DateTime.Now;
            doc["MyNull"] = null;
            doc["EmptyObj"] = new BsonDocument();
            doc["EmptyString"] = "";
            doc["maxDate"] = DateTime.MaxValue;
            doc["minDate"] = DateTime.MinValue;



            doc["Items"] = new BsonArray();
            
            doc["Items"].AsArray.Add(new BsonDocument());
            doc["Items"].AsArray[0].AsDocument["Qtd"] = 3;
            doc["Items"].AsArray[0].AsDocument["Description"] = "Big beer package";
            doc["Items"].AsArray[0].AsDocument["Unit"] = (double)10 / (double)3;
            
            doc["Items"].AsArray.Add("string-one");
            doc["Items"].AsArray.Add(null);
            doc["Items"].AsArray.Add(true);
            doc["Items"].AsArray.Add(DateTime.Now);

            doc["Last"] = 999;

            return doc;
        }

        [TestMethod]
        public void Convert_To_Json_Bson()
        {
            var o = CreateDoc();

            var bson = BsonSerializer.Serialize(o);
            var json = JsonSerializer.Serialize(o);

            var doc = BsonSerializer.Deserialize(bson);

            Assert.AreEqual(123, doc["_id"].AsInt32);
            Assert.AreEqual(o["_id"].AsInt64, doc["_id"].AsInt64);

            Assert.AreEqual(o["FirstString"].AsString, doc["FirstString"].AsString);
            Assert.AreEqual(o["Date"].AsDateTime.ToString(), doc["Date"].AsDateTime.ToString());
            Assert.AreEqual(o["CustomerId"].AsGuid, doc["CustomerId"].AsGuid);
            Assert.AreEqual(o["MyNull"].RawValue, doc["MyNull"].RawValue);
            Assert.AreEqual(o["EmptyString"].AsString, doc["EmptyString"].AsString);

            Assert.AreEqual(DateTime.MaxValue, doc["maxDate"].AsDateTime);
            Assert.AreEqual(DateTime.MinValue, doc["minDate"].AsDateTime);

            Assert.AreEqual(o["Items"].AsArray.Count, doc["Items"].AsArray.Count);
            Assert.AreEqual(o["Items"].AsArray[0].AsDocument["Unit"].AsDouble, doc["Items"].AsArray[0].AsDocument["Unit"].AsDouble);
            Assert.AreEqual(o["Items"].AsArray[4].AsDateTime.ToString(), doc["Items"].AsArray[4].AsDateTime.ToString());
        }

        [TestMethod]
        public void Bson_Using_UTC_Local_Dates()
        {
            var doc = new BsonDocument { ["now"] = DateTime.Now, ["min"] = DateTime.MinValue, ["max"] = DateTime.MaxValue };
            var bytes = BsonSerializer.Serialize(doc);

            var local = BsonSerializer.Deserialize(bytes, false);
            var utc = BsonSerializer.Deserialize(bytes, true);

            // local test
            Assert.AreEqual(DateTime.MinValue, local["min"].AsDateTime);
            Assert.AreEqual(DateTime.MaxValue, local["max"].AsDateTime);
            Assert.AreEqual(DateTimeKind.Local, local["now"].AsDateTime.Kind);

            // utc test
            Assert.AreEqual(DateTime.MinValue, utc["min"].AsDateTime);
            Assert.AreEqual(DateTime.MaxValue, utc["max"].AsDateTime);
            Assert.AreEqual(DateTimeKind.Utc, utc["now"].AsDateTime.Kind);
        }

        [TestMethod]
        public void Bson_Partial_Deserialize()
        {
            var src = this.CreateDoc();
            var bson = BsonSerializer.Serialize(src);

            // read only _id and string
            var doc1 = BsonSerializer.Deserialize(bson, false, new HashSet<string>(new string[] { "_id", "FirstString" }));

            Assert.AreEqual(src["_id"].AsInt32, doc1["_id"].AsInt32);
            Assert.AreEqual(src["FirstString"].AsString, doc1["FirstString"].AsString);

            // read only 2 sub documents
            var doc2 = BsonSerializer.Deserialize(bson, false, new HashSet<string>(new string[] { "Address", "Date" }));

            Assert.AreEqual(src["Address"].AsDocument.ToString(), doc2["Address"].AsDocument.ToString());
            Assert.AreEqual(src["Date"].AsDateTime, doc2["Date"].AsDateTime);

            // read only last field
            var doc3 = BsonSerializer.Deserialize(bson, false, new HashSet<string>(new string[] { "Last" }));

            Assert.AreEqual(src["Last"].AsInt32, doc3["Last"].AsInt32);

            // read all document
            var doc4 = BsonSerializer.Deserialize(bson, false);

            Assert.AreEqual(src.ToString(), doc4.ToString());

        }
    }
}