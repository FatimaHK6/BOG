using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class SeedHierarchicalClassifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete old flat classifications (IDs 1-6)
            migrationBuilder.DeleteData(
                table: "Classifications",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6 });

            // Insert hierarchical classifications
            // Category 1: Contracts (عقود) - IDs 7-10
            migrationBuilder.InsertData(
                table: "Classifications",
                columns: new[] { "Name", "NameAr", "Description", "Level1", "Level2", "Level3", "Level4", "IsActive", "IsDeleted", "CreatedDate", "ModifiedDate" },
                values: new object[,]
                {
                    { "RealEstateSaleContract", "عقد بيع عقار", "Sale contract for real estate", "عقود", "عقود مدنية", "عقود البيع", "عقد بيع عقار", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "ResidentialLeaseContract", "عقد إيجار سكني", "Lease contract for residential property", "عقود", "عقود مدنية", "عقود الإيجار", "عقد إيجار سكني", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "CompanyFormationContract", "عقد تأسيس شركة", "Partnership formation agreement", "عقود", "عقود تجارية", "عقود الشركات", "عقد تأسيس شركة", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "FixedTermEmploymentContract", "عقد عمل محدد المدة", "Fixed term employment agreement", "عقود", "عقود تجارية", "عقود العمل", "عقد عمل محدد المدة", true, false, DateTime.UtcNow, DateTime.UtcNow },

                    // Category 2: Civil Cases (دعاوى مدنية) - IDs 11-13
                    { "RealEstateOwnershipDispute", "نزاع ملكية عقار", "Property ownership dispute", "دعاوى مدنية", "دعاوى الملكية", "النزاعات العقارية", "نزاع ملكية عقار", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "FinancialCompensation", "تعويض عن ضرر مالي", "Claim for financial damages", "دعاوى مدنية", "دعاوى التعويض", "التعويض المالي", "تعويض عن ضرر مادي", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "CommercialDebtCollection", "دعوى تحصيل دين تجاري", "Collection of commercial debt", "دعاوى مدنية", "دعاوى الديون", "تحصيل الديون", "دعوى تحصيل دين تجاري", true, false, DateTime.UtcNow, DateTime.UtcNow },

                    // Category 3: Commercial Cases (دعاوى تجارية) - IDs 14-16
                    { "PartnershipDividendDispute", "نزاع توزيع أرباح", "Partnership profit distribution dispute", "دعاوى تجارية", "منازعات الشركات", "النزاعات بين الشركاء", "نزاع توزيع أرباح", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "BankruptcyDeclaration", "طلب إشهار إفلاس", "Bankruptcy filing request", "دعاوى تجارية", "الإفلاس والتصفية", "إجراءات الإفلاس", "طلب إشهار إفلاس", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "CommercialContractDispute", "نزاع عقد تجاري", "Commercial contract dispute", "دعاوى تجارية", "منازعات العقود", "نزاعات التنفيذ", "نزاع عقد تجاري", true, false, DateTime.UtcNow, DateTime.UtcNow },

                    // Category 4: Labor Cases (دعاوى عمالية) - IDs 17-19
                    { "UnpaidWagesClaim", "مطالبة بأجور متأخرة", "Claim for unpaid wages", "دعاوى عمالية", "حقوق العمال", "مستحقات مالية", "مطالبة بأجور متأخرة", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "UnfairTermination", "دعوى فصل تعسفي", "Unfair termination lawsuit", "دعاوى عمالية", "إنهاء الخدمة", "الفصل التعسفي", "دعوى فصل تعسفي", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "WorkplaceInjury", "دعوى إصابة عمل", "Workplace injury claim", "دعاوى عمالية", "الحقوق والواجبات", "الحماية والأمان", "دعوى إصابة عمل", true, false, DateTime.UtcNow, DateTime.UtcNow },

                    // Category 5: Family Cases (دعاوى أحوال شخصية) - IDs 20-31
                    { "MarriageCase", "دعوى زواج", "Marriage-related case", "دعاوى أحوال شخصية", "دعاوى الزواج", "", "", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "DivorceCase", "دعوى طلاق", "Divorce case", "دعاوى أحوال شخصية", "دعاوى الطلاق", "", "", true, false, DateTime.UtcNow, DateTime.UtcNow },

                    // Additional Contracts
                    { "CommercialLeaseContract", "عقد إيجار تجاري", "Commercial property lease", "عقود", "عقود مدنية", "عقود الإيجار", "عقد إيجار تجاري", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "GiftContract", "عقد هبة", "Gift agreement", "عقود", "عقود مدنية", "عقود الملكية", "عقد هبة", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "LoanContract", "عقد قرض", "Loan agreement", "عقود", "عقود مدنية", "عقود الالتزام", "عقد قرض", true, false, DateTime.UtcNow, DateTime.UtcNow },

                    // Additional Civil Cases
                    { "PersonalInjuryCompensation", "تعويض عن إصابة شخصية", "Personal injury compensation", "دعاوى مدنية", "دعاوى التعويض", "التعويض الشخصي", "تعويض عن إصابة شخصية", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "PropertyDamageCompensation", "تعويض عن تلف الملكية", "Property damage compensation", "دعاوى مدنية", "دعاوى التعويض", "تعويض عن الأضرار", "تعويض عن تلف الملكية", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "PaymentDefaultCase", "دعوى عدم السداد", "Non-payment default case", "دعاوى مدنية", "دعاوى الديون", "ديون المستهلكين", "دعوى عدم السداد", true, false, DateTime.UtcNow, DateTime.UtcNow },

                    // Additional Commercial Cases
                    { "IntellectualPropertyDispute", "نزاع الملكية الفكرية", "Intellectual property dispute", "دعاوى تجارية", "حقوق الملكية", "براءات الاختراع", "نزاع براءة اختراع", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "CompetitionLawViolation", "انتهاك قانون المنافسة", "Competition law violation", "دعاوى تجارية", "الممارسات غير العادلة", "الاحتكار والتنافس", "انتهاك قانون المنافسة", true, false, DateTime.UtcNow, DateTime.UtcNow },

                    // Additional Labor Cases
                    { "WorkplaceHarassment", "دعوى التحرش في العمل", "Workplace harassment claim", "دعاوى عمالية", "حقوق العمال", "الحقوق الشخصية", "دعوى التحرش في العمل", true, false, DateTime.UtcNow, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete all hierarchical classifications
            migrationBuilder.DeleteData(
                table: "Classifications",
                keyColumn: "Id",
                keyValues: new object[] { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 });

            // Re-insert old flat classifications (restore original 6 entries)
            migrationBuilder.InsertData(
                table: "Classifications",
                columns: new[] { "Name", "NameAr", "Description", "IsActive", "IsDeleted", "CreatedDate", "ModifiedDate" },
                values: new object[,]
                {
                    { "CivilCase", "دعوى مدنية", "Civil case", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "CommercialCase", "دعوى تجارية", "Commercial case", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "LaborCase", "دعوى عمالية", "Labor case", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "FamilyCase", "دعوى أحوال شخصية", "Family case", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "Contract", "عقد", "Contract", true, false, DateTime.UtcNow, DateTime.UtcNow },
                    { "Other", "أخرى", "Other", true, false, DateTime.UtcNow, DateTime.UtcNow }
                });
        }
    }
}
