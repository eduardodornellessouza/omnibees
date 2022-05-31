using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OB.BL.Operations.Helper.Masking;
using OB.Reservation.BL.Contracts.Attributes;

namespace OB.BL.Operations.Test.Helper.Masking
{
    [TestClass]
    public static class MaskProxyTest
    {
        [TestClass]
        public class SimpleObject
        {
            public class TestSubjectCaseA
            {
                public virtual int Integer { get; set; }
                public virtual string String { get; set; }
                public virtual IEnumerable<string> Object { get; set; }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void SimpleObject_MaskObject_Simple()
            {
                TestSubjectCaseA original = new TestSubjectCaseA
                {
                    Integer = 1,
                    String = "Test",
                    Object = Enumerable.Empty<string>(),
                };

                TestSubjectCaseA masked = new TestSubjectCaseA
                {
                    Integer = 2,
                    String = "Masked",
                    Object = Enumerable.Empty<string>(),
                };

                TestSubjectCaseA result = original.Mask(masked);

                Assert.AreEqual(masked.Integer, result.Integer);
                Assert.AreEqual(masked.String, result.String);
                Assert.AreEqual(masked.Object, result.Object);
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void SimpleObject_MaskObject_AccessOriginal()
            {
                TestSubjectCaseA original = new TestSubjectCaseA
                {
                    Integer = 1,
                    String = "Test",
                    Object = Enumerable.Empty<string>(),
                };

                TestSubjectCaseA masked = new TestSubjectCaseA
                {
                    Integer = 2,
                    String = "Masked",
                    Object = Enumerable.Empty<string>(),
                };

                TestSubjectCaseA result = original.Mask(masked);
                using (result.AsOriginal())
                {
                    Assert.AreEqual(original.Integer, result.Integer);
                    Assert.AreEqual(original.String, result.String);
                    Assert.AreEqual(original.Object, result.Object);
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void SimpleObject_MaskObject_AccessMaskedInsideOriginal()
            {
                TestSubjectCaseA original = new TestSubjectCaseA
                {
                    Integer = 1,
                    String = "Test",
                    Object = Enumerable.Empty<string>(),
                };

                TestSubjectCaseA masked = new TestSubjectCaseA
                {
                    Integer = 2,
                    String = "Masked",
                    Object = Enumerable.Empty<string>(),
                };

                TestSubjectCaseA result = original.Mask(masked);
                using (result.AsOriginal())
                {
                    using (result.AsMasked())
                    {
                        Assert.AreEqual(masked.Integer, result.Integer);
                        Assert.AreEqual(masked.String, result.String);
                        Assert.AreEqual(masked.Object, result.Object);
                    }
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void SimpleObject_MaskObject_AutoRevertAfterUsing()
            {
                TestSubjectCaseA original = new TestSubjectCaseA
                {
                    Integer = 1,
                    String = "Test",
                    Object = Enumerable.Empty<string>(),
                };

                TestSubjectCaseA masked = new TestSubjectCaseA
                {
                    Integer = 2,
                    String = "Masked",
                    Object = Enumerable.Empty<string>(),
                };

                TestSubjectCaseA result = original.Mask(masked);
                using (result.AsOriginal())
                {
                }

                Assert.AreEqual(masked.Integer, result.Integer);
                Assert.AreEqual(masked.String, result.String);
                Assert.AreEqual(masked.Object, result.Object);
            }

            [TestMethod]
            [TestCategory("Masking")]
            [DataTestMethod]
            [DataRow("378282246310005")]
            public void SimpleObject_MaskObject_WithCC(string number)
            {
                TestSubjectCaseA original = new TestSubjectCaseA
                {
                    Integer = 1,
                    String = number,
                    Object = new List<string>
                    {
                        number,
                    },
                };
                TestSubjectCaseA result = original.MaskCC();

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreNotEqual(original.String, result.String);
                CollectionAssert.AreNotEqual(original.Object.ToList(), result.Object.ToList());
            }

            [TestMethod]
            [TestCategory("Masking")]
            [DataTestMethod]
            [DataRow("aaaaaaa")]
            public void SimpleObject_MaskObject_Without_CC(string input)
            {
                TestSubjectCaseA original = new TestSubjectCaseA
                {
                    Integer = 1,
                    String = input,
                    Object = new List<string>
                {
                    input,
                },
                };
                TestSubjectCaseA result = original.MaskCC();

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(original.String, result.String);
                CollectionAssert.AreEqual(original.Object.ToList(), result.Object.ToList());
            }
        }

        [TestClass]
        public class SimpleObject_Annotation
        {
            [MaskFilter]
            public class SubTestSubject
            {
                [MaskFilter]
                public virtual string String1 { get; set; }
                public virtual string String2 { get; set; }
                public virtual long Long { get; set; }
            }

            [MaskFilter]
            public class TestSubjectCaseB
            {
                public virtual int Integer { get; set; }
                [MaskFilter]
                public virtual string String1 { get; set; }
                public virtual string String2 { get; set; }

                [RecursiveMask]
                public virtual SubTestSubject SubSubject { get; set; }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void SimpleObject_Annotation_MaskObject_Simple()
            {
                TestSubjectCaseB original = new TestSubjectCaseB
                {
                    Integer = 1,
                    String1 = "Test1",
                    String2 = "Test2",
                    SubSubject = new SubTestSubject
                    {
                        String1 = "SubTest1",
                        String2 = "SubTest2",
                        Long = 11,
                    },
                };

                TestSubjectCaseB masked = new TestSubjectCaseB
                {
                    Integer = 2,
                    String1 = "Masked1",
                    String2 = "Masked2",
                    SubSubject = new SubTestSubject
                    {
                        String1 = "MaskedSub1",
                        String2 = "MaskedSub2",
                        Long = 12,
                    },
                };

                TestSubjectCaseB result = original.Mask(masked);

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(masked.String1, result.String1);
                Assert.AreEqual(original.String2, result.String2);
                Assert.AreEqual(masked.SubSubject.String1, result.SubSubject.String1);
                Assert.AreEqual(original.SubSubject.String2, result.SubSubject.String2);
                Assert.AreEqual(original.SubSubject.Long, result.SubSubject.Long);
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void SimpleObject_Annotation_MaskObject_AccessOriginal()
            {
                TestSubjectCaseB original = new TestSubjectCaseB
                {
                    Integer = 1,
                    String1 = "Test1",
                    String2 = "Test2",
                    SubSubject = new SubTestSubject
                    {
                        String1 = "SubTest1",
                        String2 = "SubTest2",
                        Long = 11,
                    },
                };

                TestSubjectCaseB masked = new TestSubjectCaseB
                {
                    Integer = 2,
                    String1 = "Masked1",
                    String2 = "Masked2",
                    SubSubject = new SubTestSubject
                    {
                        String1 = "MaskedSub1",
                        String2 = "MaskedSub2",
                        Long = 12,
                    },
                };

                TestSubjectCaseB result = original.Mask(masked);
                using (result.AsOriginal())
                {
                    Assert.AreEqual(original.Integer, result.Integer);
                    Assert.AreEqual(original.String1, result.String1);
                    Assert.AreEqual(original.String2, result.String2);

                    Assert.AreEqual(original.SubSubject.String1, result.SubSubject.String1);
                    Assert.AreEqual(original.SubSubject.String2, result.SubSubject.String2);
                    Assert.AreEqual(original.SubSubject.Long, result.SubSubject.Long);
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void SimpleObject_Annotation_MaskObject_AccessMaskedInsideOriginal()
            {
                TestSubjectCaseB original = new TestSubjectCaseB
                {
                    Integer = 1,
                    String1 = "Test1",
                    String2 = "Test2",
                    SubSubject = new SubTestSubject
                    {
                        String1 = "SubTest1",
                        String2 = "SubTest2",
                        Long = 11,
                    },
                };

                TestSubjectCaseB masked = new TestSubjectCaseB
                {
                    Integer = 2,
                    String1 = "Masked1",
                    String2 = "Masked2",
                    SubSubject = new SubTestSubject
                    {
                        String1 = "MaskedSub1",
                        String2 = "MaskedSub2",
                        Long = 12,
                    },
                };

                TestSubjectCaseB result = original.Mask(masked);
                using (result.AsOriginal())
                {
                    using (result.AsMasked())
                    {
                        Assert.AreEqual(original.Integer, result.Integer);
                        Assert.AreEqual(masked.String1, result.String1);
                        Assert.AreEqual(original.String2, result.String2);

                        Assert.AreEqual(masked.SubSubject.String1, result.SubSubject.String1);
                        Assert.AreEqual(original.SubSubject.String2, result.SubSubject.String2);
                        Assert.AreEqual(original.SubSubject.Long, result.SubSubject.Long);
                    }
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void SimpleObject_Annotation_MaskObject_AutoRevertAfterUsing()
            {
                TestSubjectCaseB original = new TestSubjectCaseB
                {
                    Integer = 1,
                    String1 = "Test1",
                    String2 = "Test2",
                    SubSubject = new SubTestSubject
                    {
                        String1 = "SubTest1",
                        String2 = "SubTest2",
                        Long = 11,
                    },
                };

                TestSubjectCaseB masked = new TestSubjectCaseB
                {
                    Integer = 2,
                    String1 = "Masked1",
                    String2 = "Masked2",
                    SubSubject = new SubTestSubject
                    {
                        String1 = "MaskedSub1",
                        String2 = "MaskedSub2",
                        Long = 12,
                    },
                };

                TestSubjectCaseB result = original.Mask(masked);
                using (result.AsOriginal())
                {
                }

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(masked.String1, result.String1);
                Assert.AreEqual(original.String2, result.String2);

                Assert.AreEqual(masked.SubSubject.String1, result.SubSubject.String1);
                Assert.AreEqual(original.SubSubject.String2, result.SubSubject.String2);
                Assert.AreEqual(original.SubSubject.Long, result.SubSubject.Long);
            }

            [TestMethod]
            [TestCategory("Masking")]
            [DataTestMethod]
            [DataRow("378282246310005")]
            public void SimpleObject_Annotation_MaskObject_WithCC(string number)
            {
                TestSubjectCaseB original = new TestSubjectCaseB
                {
                    Integer = 1,
                    String1 = number,
                    String2 = "Test2",
                    SubSubject = new SubTestSubject
                    {
                        String1 = number,
                        String2 = "SubTest2",
                        Long = 11,
                    },
                };
                TestSubjectCaseB result = original.MaskCC();

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreNotEqual(original.String1, result.String1);
                Assert.AreNotEqual(original.SubSubject.String1, result.SubSubject.String1);
            }

            [TestMethod]
            [TestCategory("Masking")]
            [DataTestMethod]
            [DataRow("aaaaaaa")]
            public void SimpleObject_Annotation_MaskObject_Without_CC(string input)
            {
                TestSubjectCaseB original = new TestSubjectCaseB
                {
                    Integer = 1,
                    String1 = input,
                    String2 = "Test2",
                    SubSubject = new SubTestSubject
                    {
                        String1 = input,
                        String2 = "SubTest2",
                        Long = 11,
                    },
                };
                TestSubjectCaseB result = original.MaskCC();

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(original.String1, result.String1);
                Assert.AreEqual(original.SubSubject.String1, result.SubSubject.String1);
            }
        }

        [TestClass]
        public class List_Annotation
        {
            [MaskFilter]
            public class SubTestSubject
            {
                [MaskFilter]
                public virtual string String1 { get; set; }
                public virtual string String2 { get; set; }
                public virtual long Long { get; set; }
            }

            [MaskFilter]
            public class Subject
            {
                public virtual int Integer { get; set; }

                [RecursiveMask]
                public virtual IEnumerable<SubTestSubject> List { get; set; }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void List_Annotation_MaskObject_Simple()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    List = new List<SubTestSubject>
                    {
                        new SubTestSubject
                        {
                            Long = 11,
                            String1 = "Teste 1.1",
                            String2 = "Teste 1.2",
                        },
                        new SubTestSubject
                        {
                            Long = 12,
                            String1 = "Teste 2.1",
                            String2 = "Teste 2.2",
                        }
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    List = new List<SubTestSubject>
                    {
                        new SubTestSubject
                        {
                            Long = 111,
                            String1 = "Mask Teste 1.1",
                            String2 = "Mask Teste 1.2",
                        },
                        new SubTestSubject
                        {
                            Long = 112,
                            String1 = "MaskTeste 2.1",
                            String2 = "Mask Teste 2.2",
                        }
                    }
                };

                var result = original.Mask(masked);

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(original.List.Count(), result.List.Count());
                for(int i = 0; i < original.List.Count(); ++i)
                {
                    var originalItem = original.List.ElementAt(i);
                    var maskedItem = masked.List.ElementAt(i);
                    var resultItem = result.List.ElementAt(i);

                    Assert.AreEqual(maskedItem.String1, resultItem.String1);
                    Assert.AreEqual(originalItem.String2, resultItem.String2);
                    Assert.AreEqual(originalItem.Long, resultItem.Long);
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void List_Annotation_MaskObject_AccessOriginal()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    List = new List<SubTestSubject>
                    {
                        new SubTestSubject
                        {
                            Long = 11,
                            String1 = "Teste 1.1",
                            String2 = "Teste 1.2",
                        },
                        new SubTestSubject
                        {
                            Long = 12,
                            String1 = "Teste 2.1",
                            String2 = "Teste 2.2",
                        }
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    List = new List<SubTestSubject>
                    {
                        new SubTestSubject
                        {
                            Long = 111,
                            String1 = "Mask Teste 1.1",
                            String2 = "Mask Teste 1.2",
                        },
                        new SubTestSubject
                        {
                            Long = 112,
                            String1 = "MaskTeste 2.1",
                            String2 = "Mask Teste 2.2",
                        }
                    }
                };

                var result = original.Mask(masked);
                using (result.AsOriginal())
                {
                    Assert.AreEqual(original.Integer, result.Integer);

                    Assert.AreEqual(original.List.Count(), result.List.Count());
                    for (int i = 0; i < original.List.Count(); ++i)
                    {
                        var originalItem = original.List.ElementAt(i);
                        var resultItem = result.List.ElementAt(i);

                        Assert.AreEqual(originalItem.String1, resultItem.String1);
                        Assert.AreEqual(originalItem.String2, resultItem.String2);
                        Assert.AreEqual(originalItem.Long, resultItem.Long);
                    }
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void List_Annotation_MaskObject_AccessMaskedInsideOriginal()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    List = new List<SubTestSubject>
                    {
                        new SubTestSubject
                        {
                            Long = 11,
                            String1 = "Teste 1.1",
                            String2 = "Teste 1.2",
                        },
                        new SubTestSubject
                        {
                            Long = 12,
                            String1 = "Teste 2.1",
                            String2 = "Teste 2.2",
                        }
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    List = new List<SubTestSubject>
                    {
                        new SubTestSubject
                        {
                            Long = 111,
                            String1 = "Mask Teste 1.1",
                            String2 = "Mask Teste 1.2",
                        },
                        new SubTestSubject
                        {
                            Long = 112,
                            String1 = "MaskTeste 2.1",
                            String2 = "Mask Teste 2.2",
                        }
                    }
                };

                var result = original.Mask(masked);
                using (result.AsOriginal())
                {
                    using (result.AsMasked())
                    {
                        Assert.AreEqual(original.Integer, result.Integer);
                        Assert.AreEqual(original.List.Count(), result.List.Count());
                        for (int i = 0; i < original.List.Count(); ++i)
                        {
                            var originalItem = original.List.ElementAt(i);
                            var maskedItem = masked.List.ElementAt(i);
                            var resultItem = result.List.ElementAt(i);

                            Assert.AreEqual(maskedItem.String1, resultItem.String1);
                            Assert.AreEqual(originalItem.String2, resultItem.String2);
                            Assert.AreEqual(originalItem.Long, resultItem.Long);
                        }
                    }
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void List_Annotation_MaskObject_AutoRevertAfterUsing()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    List = new List<SubTestSubject>
                    {
                        new SubTestSubject
                        {
                            Long = 11,
                            String1 = "Teste 1.1",
                            String2 = "Teste 1.2",
                        },
                        new SubTestSubject
                        {
                            Long = 12,
                            String1 = "Teste 2.1",
                            String2 = "Teste 2.2",
                        }
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    List = new List<SubTestSubject>
                    {
                        new SubTestSubject
                        {
                            Long = 111,
                            String1 = "Mask Teste 1.1",
                            String2 = "Mask Teste 1.2",
                        },
                        new SubTestSubject
                        {
                            Long = 112,
                            String1 = "MaskTeste 2.1",
                            String2 = "Mask Teste 2.2",
                        }
                    }
                };

                var result = original.Mask(masked);
                using (result.AsOriginal())
                {
                }

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(original.List.Count(), result.List.Count());
                for (int i = 0; i < original.List.Count(); ++i)
                {
                    var originalItem = original.List.ElementAt(i);
                    var maskedItem = masked.List.ElementAt(i);
                    var resultItem = result.List.ElementAt(i);

                    Assert.AreEqual(maskedItem.String1, resultItem.String1);
                    Assert.AreEqual(originalItem.String2, resultItem.String2);
                    Assert.AreEqual(originalItem.Long, resultItem.Long);
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            [DataTestMethod]
            [DataRow("378282246310005")]
            public void List_Annotation_MaskObject_WithCC(string number)
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    List = new List<SubTestSubject>
                    {
                        new SubTestSubject
                        {
                            Long = 11,
                            String1 = number,
                            String2 = "Teste 1.2",
                        },
                        new SubTestSubject
                        {
                            Long = 12,
                            String1 = "Teste 2.1",
                            String2 = number,
                        }
                    }
                };
                var result = original.MaskCC();

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreNotEqual(original.List.ElementAt(0).String1, result.List.ElementAt(0).String1);
                Assert.AreEqual(original.List.ElementAt(1).String2, result.List.ElementAt(1).String2);
            }

            [TestMethod]
            [TestCategory("Masking")]
            [DataTestMethod]
            [DataRow("aaaaaaa")]
            public void List_Annotation_MaskObject_Without_CC(string input)
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    List = new List<SubTestSubject>
                    {
                        new SubTestSubject
                        {
                            Long = 11,
                            String1 = input,
                            String2 = "Teste 1.2",
                        },
                        new SubTestSubject
                        {
                            Long = 12,
                            String1 = "Teste 2.1",
                            String2 = input,
                        }
                    }
                };
                var result = original.MaskCC();

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(original.List.ElementAt(0).String1, result.List.ElementAt(0).String1);
                Assert.AreEqual(original.List.ElementAt(1).String2, result.List.ElementAt(1).String2);
            }
        }

        [TestClass]
        public class Dictionary_Value_Annotation
        {
            [MaskFilter]
            public class SubTestSubject
            {
                [MaskFilter]
                public virtual string String1 { get; set; }
                public virtual string String2 { get; set; }
                public virtual long Long { get; set; }
            }

            [MaskFilter]
            public class Subject
            {
                public virtual int Integer { get; set; }

                [RecursiveMask]
                public virtual IDictionary<int, SubTestSubject> Dictionary { get; set; }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void Dictionary_Value_Annotation_MaskObject_Simple()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    Dictionary = new Dictionary<int, SubTestSubject>
                    {
                        { 1,  new SubTestSubject {
                                Long = 11,
                                String1 = "Teste 1.1",
                                String2 = "Teste 1.2",
                            }
                        },
                        { 2, new SubTestSubject {
                                Long = 12,
                                String1 = "Teste 2.1",
                                String2 = "Teste 2.2",
                            }
                        }
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    Dictionary = new Dictionary<int, SubTestSubject>
                    {
                        { 1,  new SubTestSubject {
                                Long = 111,
                                String1 = "Mask Teste 1.1",
                                String2 = "Mask Teste 1.2",
                            }
                        },
                        { 2, new SubTestSubject {
                                Long = 112,
                                String1 = "MaskTeste 2.1",
                                String2 = "Mask Teste 2.2",
                            }
                        }
                    },
                };
                var result = original.Mask(masked);

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(original.Dictionary.Count(), result.Dictionary.Count());
                for (int i = 0; i < original.Dictionary.Count(); ++i)
                {
                    var originalEntry = original.Dictionary.ElementAt(i);
                    var maskedEntry = masked.Dictionary.ElementAt(i);
                    var resultEntry = result.Dictionary.ElementAt(i);

                    Assert.AreEqual(originalEntry.Key, resultEntry.Key);

                    Assert.AreEqual(maskedEntry.Value.String1, resultEntry.Value.String1);
                    Assert.AreEqual(originalEntry.Value.String2, resultEntry.Value.String2);
                    Assert.AreEqual(originalEntry.Value.Long, resultEntry.Value.Long);
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void Dictionary_Value_Annotation_MaskObject_AccessOriginal()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    Dictionary = new Dictionary<int, SubTestSubject>
                    {
                        { 1,  new SubTestSubject {
                                Long = 11,
                                String1 = "Teste 1.1",
                                String2 = "Teste 1.2",
                            }
                        },
                        { 2, new SubTestSubject {
                                Long = 12,
                                String1 = "Teste 2.1",
                                String2 = "Teste 2.2",
                            }
                        }
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    Dictionary = new Dictionary<int, SubTestSubject>
                    {
                        { 1,  new SubTestSubject {
                                Long = 111,
                                String1 = "Mask Teste 1.1",
                                String2 = "Mask Teste 1.2",
                            }
                        },
                        { 2, new SubTestSubject {
                                Long = 112,
                                String1 = "MaskTeste 2.1",
                                String2 = "Mask Teste 2.2",
                            }
                        }
                    },
                };

                var result = original.Mask(masked);
                using (result.AsOriginal())
                {
                    Assert.AreEqual(original.Integer, result.Integer);

                    Assert.AreEqual(original.Dictionary.Count(), result.Dictionary.Count());
                    for (int i = 0; i < original.Dictionary.Count(); ++i)
                    {
                        var originalEntry = original.Dictionary.ElementAt(i);
                        var maskedEntry = masked.Dictionary.ElementAt(i);
                        var resultEntry = result.Dictionary.ElementAt(i);

                        Assert.AreEqual(originalEntry.Key, resultEntry.Key);

                        Assert.AreEqual(originalEntry.Value.String1, resultEntry.Value.String1);
                        Assert.AreEqual(originalEntry.Value.String2, resultEntry.Value.String2);
                        Assert.AreEqual(originalEntry.Value.Long, resultEntry.Value.Long);
                    }
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void Dictionary_Value_Annotation_MaskObject_AccessMaskedInsideOriginal()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    Dictionary = new Dictionary<int, SubTestSubject>
                    {
                        { 1,  new SubTestSubject {
                                Long = 11,
                                String1 = "Teste 1.1",
                                String2 = "Teste 1.2",
                            }
                        },
                        { 2, new SubTestSubject {
                                Long = 12,
                                String1 = "Teste 2.1",
                                String2 = "Teste 2.2",
                            }
                        }
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    Dictionary = new Dictionary<int, SubTestSubject>
                    {
                        { 1,  new SubTestSubject {
                                Long = 111,
                                String1 = "Mask Teste 1.1",
                                String2 = "Mask Teste 1.2",
                            }
                        },
                        { 2, new SubTestSubject {
                                Long = 112,
                                String1 = "MaskTeste 2.1",
                                String2 = "Mask Teste 2.2",
                            }
                        }
                    },
                };

                var result = original.Mask(masked);
                using (result.AsOriginal())
                {
                    using (result.AsMasked())
                    {

                        Assert.AreEqual(original.Integer, result.Integer);
                        Assert.AreEqual(original.Dictionary.Count(), result.Dictionary.Count());
                        for (int i = 0; i < original.Dictionary.Count(); ++i)
                        {
                            var originalEntry = original.Dictionary.ElementAt(i);
                            var maskedEntry = masked.Dictionary.ElementAt(i);
                            var resultEntry = result.Dictionary.ElementAt(i);

                            Assert.AreEqual(originalEntry.Key, resultEntry.Key);

                            Assert.AreEqual(maskedEntry.Value.String1, resultEntry.Value.String1);
                            Assert.AreEqual(originalEntry.Value.String2, resultEntry.Value.String2);
                            Assert.AreEqual(originalEntry.Value.Long, resultEntry.Value.Long);
                        }
                    }
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void Dictionary_Value_Annotation_MaskObject_AutoRevertAfterUsing()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    Dictionary = new Dictionary<int, SubTestSubject>
                    {
                        { 1,  new SubTestSubject {
                                Long = 11,
                                String1 = "Teste 1.1",
                                String2 = "Teste 1.2",
                            }
                        },
                        { 2, new SubTestSubject {
                                Long = 12,
                                String1 = "Teste 2.1",
                                String2 = "Teste 2.2",
                            }
                        }
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    Dictionary = new Dictionary<int, SubTestSubject>
                    {
                        { 1,  new SubTestSubject {
                                Long = 111,
                                String1 = "Mask Teste 1.1",
                                String2 = "Mask Teste 1.2",
                            }
                        },
                        { 2, new SubTestSubject {
                                Long = 112,
                                String1 = "MaskTeste 2.1",
                                String2 = "Mask Teste 2.2",
                            }
                        }
                    },
                };

                var result = original.Mask(masked);
                using (result.AsOriginal())
                {
                }

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(original.Dictionary.Count(), result.Dictionary.Count());
                for (int i = 0; i < original.Dictionary.Count(); ++i)
                {
                    var originalEntry = original.Dictionary.ElementAt(i);
                    var maskedEntry = masked.Dictionary.ElementAt(i);
                    var resultEntry = result.Dictionary.ElementAt(i);

                    Assert.AreEqual(originalEntry.Key, resultEntry.Key);

                    Assert.AreEqual(maskedEntry.Value.String1, resultEntry.Value.String1);
                    Assert.AreEqual(originalEntry.Value.String2, resultEntry.Value.String2);
                    Assert.AreEqual(originalEntry.Value.Long, resultEntry.Value.Long);
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            [DataTestMethod]
            [DataRow("378282246310005")]
            [Ignore]
            public void Dictionary_Value_Annotation_MaskObject_WithCC(string number)
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    Dictionary = new Dictionary<int, SubTestSubject>
                    {
                        { 1,  new SubTestSubject {
                                Long = 11,
                                String1 = number,
                                String2 = "Teste 1.2",
                            }
                        },
                        { 2, new SubTestSubject {
                                Long = 12,
                                String1 = "Teste 2.1",
                                String2 = number,
                            }
                        }
                    }
                };
                var result = original.MaskCC();

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreNotEqual(original.Dictionary.Values.ElementAt(0).String1, result.Dictionary.Values.ElementAt(0).String1);
                Assert.AreEqual(original.Dictionary.Values.ElementAt(1).String2, result.Dictionary.Values.ElementAt(1).String2);
            }

            [TestMethod]
            [TestCategory("Masking")]
            [DataTestMethod]
            [DataRow("aaaaaaa")]
            public void Dictionary_Value_Annotation_MaskObject_Without_CC(string input)
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    Dictionary = new Dictionary<int, SubTestSubject>
                    {
                        { 1,  new SubTestSubject {
                                Long = 11,
                                String1 = input,
                                String2 = "Teste 1.2",
                            }
                        },
                        { 2, new SubTestSubject {
                                Long = 12,
                                String1 = "Teste 2.1",
                                String2 = input,
                            }
                        }
                    }
                };
                var result = original.MaskCC();

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(original.Dictionary.Values.ElementAt(0).String1, result.Dictionary.Values.ElementAt(0).String1);
                Assert.AreEqual(original.Dictionary.Values.ElementAt(1).String2, result.Dictionary.Values.ElementAt(1).String2);
            }
        }

        [TestClass]
        public class Dictionary_Key_Annotation
        {
            [MaskFilter]
            public class SubTestSubject
            {
                [MaskFilter]
                public virtual string String1 { get; set; }
                public virtual string String2 { get; set; }
                public virtual long Long { get; set; }
            }

            [MaskFilter]
            public class Subject
            {
                public virtual int Integer { get; set; }

                [RecursiveMask]
                public virtual IDictionary<SubTestSubject, int> Dictionary { get; set; }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void Dictionary_Key_Annotation_MaskObject_Simple()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    Dictionary = new Dictionary<SubTestSubject, int>
                    {
                        {new SubTestSubject {
                            Long = 11,
                            String1 = "Teste 1.1",
                            String2 = "Teste 1.2",
                        }, 1 },
                        { new SubTestSubject {
                                Long = 12,
                                String1 = "Teste 2.1",
                                String2 = "Teste 2.2",
                        }, 2}
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    Dictionary = new Dictionary<SubTestSubject, int>
                    {
                        {new SubTestSubject {
                            Long = 111,
                            String1 = "Mask Teste 1.1",
                            String2 = "Mask Teste 1.2",
                        }, 1},
                        {new SubTestSubject {
                            Long = 112,
                            String1 = "MaskTeste 2.1",
                            String2 = "Mask Teste 2.2",
                        }, 2}
                    },
                };
                var result = original.Mask(masked);

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(original.Dictionary.Count(), result.Dictionary.Count());
                for (int i = 0; i < original.Dictionary.Count(); ++i)
                {
                    var originalEntry = original.Dictionary.ElementAt(i);
                    var maskedEntry = masked.Dictionary.ElementAt(i);
                    var resultEntry = result.Dictionary.ElementAt(i);

                    Assert.AreEqual(originalEntry.Value, resultEntry.Value);

                    Assert.AreEqual(maskedEntry.Key.String1, resultEntry.Key.String1);
                    Assert.AreEqual(originalEntry.Key.String2, resultEntry.Key.String2);
                    Assert.AreEqual(originalEntry.Key.Long, resultEntry.Key.Long);
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void Dictionary_Key_Annotation_MaskObject_AccessOriginal()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    Dictionary = new Dictionary<SubTestSubject, int>
                    {
                        {new SubTestSubject {
                            Long = 11,
                            String1 = "Teste 1.1",
                            String2 = "Teste 1.2",
                        }, 1 },
                        { new SubTestSubject {
                                Long = 12,
                                String1 = "Teste 2.1",
                                String2 = "Teste 2.2",
                        }, 2}
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    Dictionary = new Dictionary<SubTestSubject, int>
                    {
                        {new SubTestSubject {
                            Long = 111,
                            String1 = "Mask Teste 1.1",
                            String2 = "Mask Teste 1.2",
                        }, 1},
                        {new SubTestSubject {
                            Long = 112,
                            String1 = "MaskTeste 2.1",
                            String2 = "Mask Teste 2.2",
                        }, 2}
                    },
                };

                var result = original.Mask(masked);
                using (result.AsOriginal())
                {
                    Assert.AreEqual(original.Integer, result.Integer);

                    Assert.AreEqual(original.Dictionary.Count(), result.Dictionary.Count());
                    for (int i = 0; i < original.Dictionary.Count(); ++i)
                    {
                        var originalEntry = original.Dictionary.ElementAt(i);
                        var maskedEntry = masked.Dictionary.ElementAt(i);
                        var resultEntry = result.Dictionary.ElementAt(i);

                        Assert.AreEqual(originalEntry.Value, resultEntry.Value);

                        Assert.AreEqual(originalEntry.Key.String1, resultEntry.Key.String1);
                        Assert.AreEqual(originalEntry.Key.String2, resultEntry.Key.String2);
                        Assert.AreEqual(originalEntry.Key.Long, resultEntry.Key.Long);
                    }
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void Dictionary_Key_Annotation_MaskObject_AccessMaskedInsideOriginal()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    Dictionary = new Dictionary<SubTestSubject, int>
                    {
                        {new SubTestSubject {
                            Long = 11,
                            String1 = "Teste 1.1",
                            String2 = "Teste 1.2",
                        }, 1 },
                        { new SubTestSubject {
                                Long = 12,
                                String1 = "Teste 2.1",
                                String2 = "Teste 2.2",
                        }, 2}
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    Dictionary = new Dictionary<SubTestSubject, int>
                    {
                        {new SubTestSubject {
                            Long = 111,
                            String1 = "Mask Teste 1.1",
                            String2 = "Mask Teste 1.2",
                        }, 1},
                        {new SubTestSubject {
                            Long = 112,
                            String1 = "MaskTeste 2.1",
                            String2 = "Mask Teste 2.2",
                        }, 2}
                    },
                };

                var result = original.Mask(masked);
                using (result.AsOriginal())
                {
                    using (result.AsMasked())
                    {
                        Assert.AreEqual(original.Integer, result.Integer);
                        Assert.AreEqual(original.Dictionary.Count(), result.Dictionary.Count());
                        for (int i = 0; i < original.Dictionary.Count(); ++i)
                        {
                            var originalEntry = original.Dictionary.ElementAt(i);
                            var maskedEntry = masked.Dictionary.ElementAt(i);
                            var resultEntry = result.Dictionary.ElementAt(i);

                            Assert.AreEqual(originalEntry.Value, resultEntry.Value);

                            Assert.AreEqual(maskedEntry.Key.String1, resultEntry.Key.String1);
                            Assert.AreEqual(originalEntry.Key.String2, resultEntry.Key.String2);
                            Assert.AreEqual(originalEntry.Key.Long, resultEntry.Key.Long);
                        }
                    }
                }
            }

            [TestMethod]
            [TestCategory("Masking")]
            public void Dictionary_Key_Annotation_MaskObject_AutoRevertAfterUsing()
            {
                Subject original = new Subject
                {
                    Integer = 1,
                    Dictionary = new Dictionary<SubTestSubject, int>
                    {
                        {new SubTestSubject {
                            Long = 11,
                            String1 = "Teste 1.1",
                            String2 = "Teste 1.2",
                        }, 1 },
                        { new SubTestSubject {
                                Long = 12,
                                String1 = "Teste 2.1",
                                String2 = "Teste 2.2",
                        }, 2}
                    }
                };

                Subject masked = new Subject
                {
                    Integer = 11,
                    Dictionary = new Dictionary<SubTestSubject, int>
                    {
                        {new SubTestSubject {
                            Long = 111,
                            String1 = "Mask Teste 1.1",
                            String2 = "Mask Teste 1.2",
                        }, 1},
                        {new SubTestSubject {
                            Long = 112,
                            String1 = "MaskTeste 2.1",
                            String2 = "Mask Teste 2.2",
                        }, 2}
                    },
                };

                var result = original.Mask(masked);
                using (result.AsOriginal())
                {
                }

                Assert.AreEqual(original.Integer, result.Integer);
                Assert.AreEqual(original.Dictionary.Count(), result.Dictionary.Count());
                for (int i = 0; i < original.Dictionary.Count(); ++i)
                {
                    var originalEntry = original.Dictionary.ElementAt(i);
                    var maskedEntry = masked.Dictionary.ElementAt(i);
                    var resultEntry = result.Dictionary.ElementAt(i);

                    Assert.AreEqual(originalEntry.Value, resultEntry.Value);

                    Assert.AreEqual(maskedEntry.Key.String1, resultEntry.Key.String1);
                    Assert.AreEqual(originalEntry.Key.String2, resultEntry.Key.String2);
                    Assert.AreEqual(originalEntry.Key.Long, resultEntry.Key.Long);
                }
            }
        }
    }
}
