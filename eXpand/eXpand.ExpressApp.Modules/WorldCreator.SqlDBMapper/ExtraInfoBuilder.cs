﻿using System.Linq;
using DevExpress.ExpressApp;
using eXpand.Persistent.Base.PersistentMetaData;
using eXpand.Persistent.Base.PersistentMetaData.PersistentAttributeInfos;
using Microsoft.SqlServer.Management.Smo;
using eXpand.ExpressApp.WorldCreator.Core;

namespace eXpand.ExpressApp.WorldCreator.SqlDBMapper
{
    public class ExtraInfoBuilder
    {
        readonly ObjectSpace _objectSpace;
        readonly AttributeMapper _attributeMapper;

        public ExtraInfoBuilder(ObjectSpace objectSpace, AttributeMapper attributeMapper) {
            _attributeMapper = attributeMapper;
            _objectSpace = objectSpace;
        }

        public void CreateCollection(IPersistentReferenceMemberInfo persistentReferenceMemberInfo, IPersistentClassInfo owner)
        {
            var persistentCollectionMemberInfo = _objectSpace.CreateWCObject<IPersistentCollectionMemberInfo>();
            persistentReferenceMemberInfo.ReferenceClassInfo.OwnMembers.Add(persistentCollectionMemberInfo);
            persistentCollectionMemberInfo.Name = persistentReferenceMemberInfo.Owner.Name + persistentReferenceMemberInfo.Name + "s";
            persistentCollectionMemberInfo.Owner = persistentReferenceMemberInfo.ReferenceClassInfo;
            persistentCollectionMemberInfo.CollectionClassInfo = owner;
            persistentCollectionMemberInfo.SetDefaultTemplate(TemplateType.XPCollectionMember);
            var persistentAssociationAttribute = _objectSpace.CreateWCObject<IPersistentAssociationAttribute>();
            persistentCollectionMemberInfo.TypeAttributes.Add(persistentAssociationAttribute);
            persistentAssociationAttribute.AssociationName = persistentReferenceMemberInfo.TypeAttributes.OfType<IPersistentAssociationAttribute>().Single().AssociationName;
        }

        public void CreateExtraInfos(Table table, IPersistentClassInfo persistentClassInfo){
            var persistentAttributeInfos = _attributeMapper.Create(table, persistentClassInfo);
            foreach (var persistentAttributeInfo in persistentAttributeInfos){
                persistentClassInfo.TypeAttributes.Add(persistentAttributeInfo);
            }
        }

        public void CreateExtraInfos(Column column, IPersistentMemberInfo persistentMemberInfo, ForeignKeyCalculator foreignKeyCalculator)
        {
            var persistentReferenceMemberInfo = (IPersistentReferenceMemberInfo)persistentMemberInfo;
            if (persistentMemberInfo.CodeTemplateInfo.CodeTemplate.TemplateType == TemplateType.XPOneToOnePropertyMember){
                CreateTemplateInfo(persistentReferenceMemberInfo, column,foreignKeyCalculator);
            }
            else if (!column.InPrimaryKey && persistentMemberInfo.CodeTemplateInfo.CodeTemplate.TemplateType == TemplateType.XPReadWritePropertyMember)
                CreateCollection(persistentReferenceMemberInfo, persistentReferenceMemberInfo.Owner);

        }
        void CreateTemplateInfo(IPersistentReferenceMemberInfo persistentReferenceMemberInfo, Column column, ForeignKeyCalculator _foreignKeyCalculator)
        {
            var table = (Table)column.Parent;
            var database = table.Parent;
            var foreignKey = _foreignKeyCalculator.GetForeignKey(database, column.Name, table.Name);
            var templateInfo = _objectSpace.CreateWCObject<ITemplateInfo>();
            templateInfo.Name = persistentReferenceMemberInfo.CodeTemplateInfo.CodeTemplate.TemplateType.ToString();
            templateInfo.TemplateCode =
                _foreignKeyCalculator.GetRefTableForeignKey(foreignKey, column.Name).Columns.OfType<ForeignKeyColumn>().
                    Where(keyColumn => keyColumn.ReferencedColumn == column.Name).Single().Name;

            persistentReferenceMemberInfo.TemplateInfos.Add(templateInfo);
        }

    }
}
