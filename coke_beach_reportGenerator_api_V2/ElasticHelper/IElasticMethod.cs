using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.ElasticHelper
{
    public class ElasticMethod
    {

        public QueryContainer GetQuery(Dictionary<string, object> param, string innerOperatoorType, string outerOperatoorType, double lessThanSec)
        {
            if (ESBaseModel.ESLogicOperator.AND.ToString() == innerOperatoorType && ESBaseModel.ESLogicOperator.AND.ToString() == outerOperatoorType)
            {

                List<QueryContainer> innerQuery = new List<QueryContainer>();
                foreach (var item in param)
                {
                    foreach (var subItem in (List<Object>)item.Value)
                    {
                        innerQuery.Add(Query<Object>.Terms(t => t.Field(item.Key).Terms((List<int>)subItem)));
                    }
                }
                if (lessThanSec > 0)
                {
                    innerQuery.Add(Query<Object>.Range(r => r.Field("TimeStampMil").LessThan(lessThanSec)));
                }
                return Query<object>.Bool(bl => bl.Must(innerQuery.ToArray()));

            }
            else if (ESBaseModel.ESLogicOperator.AND.ToString() == innerOperatoorType && ESBaseModel.ESLogicOperator.OR.ToString() == outerOperatoorType)
            {
                List<Nest.QueryContainer> innerQuery = new List<Nest.QueryContainer>();
                foreach (var item in param)
                {
                    foreach (var subItem in (List<Object>)item.Value)
                    {
                        innerQuery.Add(Query<Object>.Terms(t => t.Field(item.Key).Terms((List<int>)subItem)));
                    }
                }

                return Query<object>.Bool(bl => bl.Should(innerQuery.ToArray()));
            }
            else if (ESBaseModel.ESLogicOperator.OR.ToString() == innerOperatoorType && ESBaseModel.ESLogicOperator.AND.ToString() == outerOperatoorType)
            {

                List<QueryContainer> innerQuery = new List<QueryContainer>();
                /*foreach (var item in param)
                {
                    Type targetType = item.Value.GetType();
                    var convertedValue = Convert.ChangeType(item.Value, targetType);
                    if (targetType.ToString().ToLower() != "system.string")
                    {
                        convertedValue = (List<int>)item.Value;
                        innerQuery.Add(Query<Object>.Terms(t => t.Field(item.Key).Terms((List<int>)item.Value)));
                    }
                    else
                    {
                        innerQuery.Add(Query<Object>.Term(t => t.Field(item.Key).Value((string)item.Value)));
                    }
                }*/
                foreach (var item in param)
                {
                    Type targetType = item.Value.GetType();
                    var convertedValue = Convert.ChangeType(item.Value, targetType);
                    bool isList = false;
                    string listType = "";
                    if (item.Value is System.Collections.IList)
                    {
                        isList = true;
                    }
                    if (isList)
                    {
                        listType = item.Value.GetType().GetGenericArguments()[0].FullName;
                    }


                    if (isList)
                    {
                        if (listType.ToString().ToLower() != "system.string")
                        {
                            innerQuery.Add(Query<Object>.Terms(t => t.Field(item.Key).Terms((List<int>)item.Value)));
                        }
                        else
                        {
                            innerQuery.Add(Query<Object>.Terms(t => t.Field(item.Key).Terms((List<string>)item.Value)));
                        }
                    }
                    else
                    {
                        if (targetType.ToString().ToLower() != "system.string")
                        {
                            convertedValue = (List<int>)item.Value;
                            innerQuery.Add(Query<Object>.Terms(t => t.Field(item.Key).Terms((List<int>)item.Value)));
                        }
                        else
                        {
                            innerQuery.Add(Query<Object>.Term(t => t.Field(item.Key).Value((string)item.Value)));
                        }
                    }

                }
                if (lessThanSec > 0)
                {
                    innerQuery.Add(Query<Object>.Range(r => r.Field("TimeStampMil")
                    .LessThanOrEquals(lessThanSec)));
                }

                return Query<object>.Bool(bl => bl.Must(innerQuery.ToArray()));
            }
            else
            {
                List<QueryContainer> termQuery = new List<QueryContainer>();

                foreach (var item in param)
                {
                    Type targetType = item.Value.GetType();
                    var convertedValue = Convert.ChangeType(item.Value, targetType);
                    bool isList = false;
                    string listType = item.Value.GetType().GetGenericArguments()[0].FullName;
                    if (listType.ToString().ToLower() != "system.string")
                    {
                        termQuery.Add(Query<Object>.Terms(t => t.Field(item.Key).Terms((List<int>)item.Value)));
                    }
                    else
                    {
                        termQuery.Add(Query<Object>.Terms(t => t.Field(item.Key).Terms((List<string>)item.Value)));
                    }

                }
                return Query<object>.Bool(bl => bl.Should(termQuery.ToArray()));
            }

        }

        public AggregationContainerDescriptor<object> GetAggregationQuery(Dictionary<string, Dictionary<string, string>> selections, List<string> compositeField, string sumField, string cntField)
        {
            AggregationContainerDescriptor<object> aggregationContainerDescriptor = new AggregationContainerDescriptor<object>();
            FilterAggregationDescriptor<object> aggregationContainer = new FilterAggregationDescriptor<object>();
            foreach (var key in selections.Keys)
            {
                var selectionName = key;
                var param = new Dictionary<string, object>();
                string outerOperator = "AND";
                string innerOperator = "OR";
                foreach (var innerKey in selections[key].Keys)
                {
                    param.Add(innerKey, new List<string> { selections[key][innerKey] });
                }
                FilterAggregationDescriptor<object> filterCompositeAgg = new FilterAggregationDescriptor<object>();
                AggregationContainerDescriptor<object> compositeAgg = new AggregationContainerDescriptor<object>();
                CompositeAggregationSourcesDescriptor<object> compositeDes = new CompositeAggregationSourcesDescriptor<object>();
                foreach (var compField in compositeField)
                {
                    compositeDes = compositeDes.Terms(compField, f => f.Field(compField));
                }
                aggregationContainerDescriptor = aggregationContainerDescriptor
                    .Filter(selectionName,
                                           f => f.Filter(a => GetQuery(param, innerOperator, outerOperator, 0))
                                               .Aggregations(a =>
                                                             a.Composite("numerator", c => c.Size(1000)
                                                                                          .Sources(s => compositeDes)
                                                                                          .Aggregations(sg => sg
                                                                                                               .Sum("weightSum", sum => sum.Field(sumField))
                                                                                                               .ValueCount("colCount", v => v.Field(cntField))
                                                                                                        )
                                                                        )
                                                             )
                           );

            }
            return aggregationContainerDescriptor;
        }

        public AggregationContainerDescriptor<object> GetAggregationQuery(Dictionary<string, Dictionary<string, string>> selections, string sumField, string cntField)
        {
            AggregationContainerDescriptor<object> aggregationContainerDescriptor = new AggregationContainerDescriptor<object>();
            FilterAggregationDescriptor<object> aggregationContainer = new FilterAggregationDescriptor<object>();
            foreach (var key in selections.Keys)
            {
                var selectionName = key;
                var param = new Dictionary<string, object>();
                string outerOperator = "AND";
                string innerOperator = "OR";
                foreach (var innerKey in selections[key].Keys)
                {
                    param.Add(innerKey, new List<string> { selections[key][innerKey] });
                }
                FilterAggregationDescriptor<object> filterCompositeAgg = new FilterAggregationDescriptor<object>();
                AggregationContainerDescriptor<object> compositeAgg = new AggregationContainerDescriptor<object>();
                CompositeAggregationSourcesDescriptor<object> compositeDes = new CompositeAggregationSourcesDescriptor<object>();
               
                aggregationContainerDescriptor = aggregationContainerDescriptor
                    .Filter(selectionName,
                                           f => f.Filter(a => GetQuery(param, innerOperator, outerOperator, 0))
                                               .Aggregations(a =>a
                                                            
                                                                                                               .Sum("weightSum", sum => sum.Field(sumField))
                                                                                                               .ValueCount("colCount", v => v.Field(cntField))
                                                                                                        
                                                                        )
                                                          
                           );

            }
            return aggregationContainerDescriptor;
        }

        public AggregationContainerDescriptor<object> GetAggregationQuery(Dictionary<string, Dictionary<string, string>> selections, List<string> compositeField)
        {
            AggregationContainerDescriptor<object> aggregationContainerDescriptor = new AggregationContainerDescriptor<object>();
            FilterAggregationDescriptor<object> aggregationContainer = new FilterAggregationDescriptor<object>();
            foreach (var key in selections.Keys)
            {
                var selectionName = key;
                var param = new Dictionary<string, object>();
                string outerOperator = "AND";
                string innerOperator = "OR";
                foreach (var innerKey in selections[key].Keys)
                {
                    param.Add(innerKey, new List<string> { selections[key][innerKey] });
                }
                FilterAggregationDescriptor<object> filterCompositeAgg = new FilterAggregationDescriptor<object>();
                AggregationContainerDescriptor<object> compositeAgg = new AggregationContainerDescriptor<object>();
                CompositeAggregationSourcesDescriptor<object> compositeDes = new CompositeAggregationSourcesDescriptor<object>();
                foreach (var compField in compositeField)
                {
                    compositeDes = compositeDes.Terms(compField, f => f.Field(compField));
                }
                aggregationContainerDescriptor = aggregationContainerDescriptor
                    .Filter(selectionName,
                                           f => f.Filter(a => GetQuery(param, innerOperator, outerOperator, 0))
                                               .Aggregations(a =>
                                                             a.Composite("numerator", c => c.Size(100000)
                                                                                          .Sources(s => compositeDes)

                                                                        )
                                                             )
                           );

            }
            return aggregationContainerDescriptor;
        }

        public AggregationContainerDescriptor<object> GetAggregationQuery(Dictionary<string, Dictionary<string, string>> selections, List<string> compositeField, string CompositeGroupByName)
        {
            AggregationContainerDescriptor<object> aggregationContainerDescriptor = new AggregationContainerDescriptor<object>();
            FilterAggregationDescriptor<object> aggregationContainer = new FilterAggregationDescriptor<object>();
            foreach (var key in selections.Keys)
            {
                var selectionName = key;
                var param = new Dictionary<string, object>();
                string outerOperator = "AND";
                string innerOperator = "OR";
                foreach (var innerKey in selections[key].Keys)
                {
                    param.Add(innerKey, new List<string> { selections[key][innerKey] });
                }
                FilterAggregationDescriptor<object> filterCompositeAgg = new FilterAggregationDescriptor<object>();
                AggregationContainerDescriptor<object> compositeAgg = new AggregationContainerDescriptor<object>();
                CompositeAggregationSourcesDescriptor<object> compositeDes = new CompositeAggregationSourcesDescriptor<object>();
                foreach (var compField in compositeField)
                {
                    compositeDes = compositeDes.Terms(compField, f => f.Field(compField));
                }
                aggregationContainerDescriptor = aggregationContainerDescriptor
                    .Filter(selectionName,
                                           f => f.Filter(a => GetQuery(param, innerOperator, outerOperator, 0))
                                               .Aggregations(a =>
                                                             a.Composite(CompositeGroupByName, c => c.Size(200000)
                                                                                          .Sources(s => compositeDes)

                                                                        )
                                                             )
                           );

            }
            return aggregationContainerDescriptor;
        }

        public AggregationContainerDescriptor<object> GetCustomAggregationQuery(Dictionary<string, Dictionary<string, string>> selections, List<string> compositeField)
        {
            AggregationContainerDescriptor<object> aggregationContainerDescriptor = new AggregationContainerDescriptor<object>();
            FilterAggregationDescriptor<object> aggregationContainer = new FilterAggregationDescriptor<object>();
            foreach (var key in selections.Keys)
            {
                var selectionName = key;
                var param = new Dictionary<string, object>();
                string outerOperator = "AND";
                string innerOperator = "OR";
                foreach (var innerKey in selections[key].Keys)
                {
                    param.Add(innerKey, new List<string> { selections[key][innerKey] });
                }
                FilterAggregationDescriptor<object> filterCompositeAgg = new FilterAggregationDescriptor<object>();
                AggregationContainerDescriptor<object> compositeAgg = new AggregationContainerDescriptor<object>();
                CompositeAggregationSourcesDescriptor<object> compositeDes = new CompositeAggregationSourcesDescriptor<object>();
                foreach (var compField in compositeField)
                {
                    compositeDes = compositeDes.Terms(compField, f => f.Field(compField));
                }
                aggregationContainerDescriptor = aggregationContainerDescriptor
                    .Filter(selectionName,
                                           f => f.Filter(a => GetQuery(param, innerOperator, outerOperator, 0))
                                               .Aggregations(a =>
                                                             a.Composite("reportedDrinks", c => c.Size(200000)
                                                                                          .Sources(s => compositeDes)

                                                                        ).Sum("productAggregation", s => GetProductAggregation("Weight", "drinks"))
                                                             )
                           ); ;

            }
            return aggregationContainerDescriptor;
        }


        public FilterAggregationDescriptor<object> GetAggregation(QueryContainer query, IPromise<IList<ICompositeAggregationSource>> compositeAggrgation, string CompositeName, string sumField, string cntField)
        {
            AggregationContainerDescriptor<object> aggregationContainerDescriptor = new AggregationContainerDescriptor<object>();
            FilterAggregationDescriptor<object> aggregationContainer = new FilterAggregationDescriptor<object>();
            aggregationContainer

            .Filter(fl => query)
            .Aggregations(subAgg => subAgg
            .Composite(CompositeName, comp => comp
            .Size(1000)
            .Sources(s => compositeAggrgation)
            .Aggregations(sg => sg
                              .Sum("weightSum", sum => sum.Field(sumField))
                              .ValueCount("colCount", v => v.Field(cntField))
                              .TopHits("hitsData", data => data)
                              )
            )
            );
            return aggregationContainer;

        }
        public FilterAggregationDescriptor<object> GetAggregation(QueryContainer query, IPromise<IList<ICompositeAggregationSource>> compositeAggrgation, string CompositeName, string sumField, string cntField, ScriptedMetricAggregationDescriptor<object> squareAggregation)
        {
            AggregationContainerDescriptor<object> aggregationContainerDescriptor = new AggregationContainerDescriptor<object>();
            FilterAggregationDescriptor<object> aggregationContainer = new FilterAggregationDescriptor<object>();
            aggregationContainer
            .Filter(fl => query)
            .Aggregations(subAgg => subAgg
            .Composite(CompositeName, comp => comp
            .Size(1000)
            .Sources(s => compositeAggrgation)
            .Aggregations(sg => sg
                              .Sum("weightSum", sum => sum.Field(sumField))
                              .ValueCount("colCount", v => v.Field(cntField))
                              .ScriptedMetric("squareAggregation", sm => squareAggregation)
                              .TopHits("hitsData", data => data)
                              )
            )
            );
            return aggregationContainer;

        }
        public FilterAggregationDescriptor<object> GetDistinctAggregation(QueryContainer query, IPromise<IList<ICompositeAggregationSource>> compositeAggrgation, string CompositeName)
        {
            AggregationContainerDescriptor<object> aggregationContainerDescriptor = new AggregationContainerDescriptor<object>();
            FilterAggregationDescriptor<object> aggregationContainer = new FilterAggregationDescriptor<object>();

            aggregationContainer
            .Filter(fl => query)
            .Aggregations(subAgg => subAgg
            .Composite(CompositeName, comp => comp
            .Sources(s => compositeAggrgation)
            .Aggregations(sg => sg.TopHits("hitsData", data => data))
            )
            );
            return aggregationContainer;

        }
        public FilterAggregationDescriptor<object> GetDynamicAggregation(QueryContainer query, IPromise<IList<ICompositeAggregationSource>> compositeAggrgation, string CompositeName, AggregationContainerDescriptor<object> aggList)
        {

            FilterAggregationDescriptor<object> aggregationContainer = new FilterAggregationDescriptor<object>();
            aggregationContainer
            .Filter(fl => query)
            .Aggregations(subAgg => subAgg
            .Composite(CompositeName, comp => comp
            .Sources(s => compositeAggrgation)
            .Aggregations(sg => aggList.TopHits("hitsData", data => data))
            )
            );
            return aggregationContainer;
        }
        public ScriptedMetricAggregationDescriptor<object> GetSquareAggregation(string fieldName)
        {
            string script = "Math.pow(doc['" + fieldName + "'].value, 2)";
            ScriptedMetricAggregationDescriptor<object> scriptedMetric = new Nest.ScriptedMetricAggregationDescriptor<object>();
            return scriptedMetric.InitScript("state.sum = 0;").MapScript("state.sum += " + script + "")
                                .CombineScript("return state.sum;")
                                .ReduceScript("double sum = 0; for (state in states) { sum += state; } return sum;");
        }

        public SumAggregationDescriptor<object> GetProductAggregation(string fieldName1, string fieldName2)
        {
            Dictionary<string, object> scriptParams = new Dictionary<string, object>();
            scriptParams.Add("v0", fieldName1);
            scriptParams.Add("v1", fieldName2);
            string script = "InternalSqlScriptUtils.mul(InternalQlScriptUtils.docValue(doc,params.v0),InternalQlScriptUtils.docValue(doc,params.v1))";
            ScriptedMetricAggregationDescriptor<object> scriptedMetric = new Nest.ScriptedMetricAggregationDescriptor<object>();
            SumAggregationDescriptor<object> sumAggregation = new SumAggregationDescriptor<object>();
            return sumAggregation.Script((s => s.Source(script).Lang("painless").Params(scriptParams)));
        }

    }
}
