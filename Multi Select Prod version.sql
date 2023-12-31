/****** Object:  StoredProcedure [dbo].[SPMultiSelectwithEquitywithSingleSelect_V3]    Script Date: 7/4/2023 12:20:30 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
																		/*CODE STARTS HERE*/
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

ALTER                       Procedure [dbo].[SPMultiSelectwithEquitywithSingleSelect_V3]
	@GeoSelections GeographySelection READONLY,
	@timeperiodid int,
	@BenchmarkComparisonSelections BenchmarkComparisonSelection READONLY,
	@DemographySelections DemographyFilters READONLY,
	@BeverageSelections BeverageFIlterNew READONLY
	with recompile
as 
begin
	Declare @OUID int
	Declare @CountryID int
	Declare @RegionID int
	Declare @BottlerID int
	select @OUID=OUID FROM @GeoSelections
	select @CountryID=CountryID FROM @GeoSelections
	select @RegionID=RegionID FROM @GeoSelections
	select @BottlerID=BottlerID FROM @GeoSelections

	Declare @isIndiaSelected bit = iif((@OUID is not null and @OUID=6) or (@COuntryID is not null and @COuntryID=18),1,0)

-------------------------------------------------------------Country Selection-------------------------------------------------------------
drop table if exists #country
Create Table #country (CountryID int)
IF exists(Select Distinct Countryid from [dbo].[DimGeographyMapping](nolock) where (ouid=@OUID or countryid=@CountryID or regionid=@RegionID or BottlerID=@BottlerID))
	begin
		insert into #country 
		Select Distinct Countryid from [dbo].[DimGeographyMapping](nolock) 
		where (ouid=@OUID or countryid=@CountryID)
		order by countryid asc
	end
else if exists(Select Distinct Countryid from [dbo].[DimGeographyMapping](nolock) where (-1=@OUID and -1=@CountryID and -1=@RegionID and -1=@BottlerID))
	Begin 
		insert into #country
		Select Distinct Countryid
		from [dbo].[DimGeographyMapping](nolock) order by countryid asc
	end 
else 
Begin 
		insert into #country
		Select Distinct Countryid
		from [dbo].[DimGeographyMapping](nolock) order by countryid asc
	end   


create table  #countryData (CountryID int,country_ID nvarchar(100))
insert into  #countryData
select distinct a.COuntryID,a.CountryName from [dbo].[DimGeographyMapping](nolock) a inner join #country b on a.COuntryID=b.COuntryID
-------------------------------------------------------------TimePeriod Selection-------------------------------------------------------------
drop table if exists #timeperiod
Create Table #timeperiod (Timeperiod int)
insert into #timeperiod
Select TImeperiod from [dbo].[DimTimePeriod](nolock)
where id=@timeperiodid

declare @timeperiodSelected bigint=-1

if(exists(select top 1 Timeperiod from [dbo].[DimTimePeriod] where timeperiodtype in ('12MMT') and id=@timeperiodid))
begin
    set  @timeperiodSelected = (select top 1 Timeperiod from [dbo].[DimTimePeriod] where timeperiodtype in ('12MMT') and id=@timeperiodid  and  ActualMonth=Month)
end
else if(exists(select top 1  Timeperiod from [dbo].[DimTimePeriod] where timeperiodtype in ('YEAR') and id=@timeperiodid))
begin
    set  @timeperiodSelected = cast(concat((select top 1 ActualYear from [dbo].[DimTimePeriod] where timeperiodtype in ('YEAR') and id=@timeperiodid ),'12') as bigint)
end

-------------------------------------------------------------Filter Data-------------------------------------------------------------

declare @TimeperiodFilter nvarchar(max)= (select string_agg(Timeperiod,',') from #timeperiod)
drop table if Exists #AddlFilter
create table #AddlFilter(index_resp bigint)
create table #AddlFilterEquity(index_resp bigint)

-------------------------------------------------------------Demog Filter Data-------------------------------------------------------------
create table #DemographicFilter(index_resp bigint)

-------------------------------------------------------------without Beverage Filter Data-------------------------------------------------------------
create table #FilterWithoutBeverage(index_resp bigint)

-------------------------------------------------------------Time Geography Filter Data-------------------------------------------------------------
create table #FilterTimeGeography(index_resp bigint)

create table #singleSelectFilterAdded(index_resp_iterator bigint)

select * into #multiSelectFilterAdded from [dbo].[MultiSelectTable](nolock) where 1=0

exec Get_AdvFilterData_MultiSelect @TimeperiodFilter,@GeoSelections,@DemographySelections,@BeverageSelections

declare @isMultiSelectFilterSelected bit=0
if(exists(select top 1 * from #multiSelectFilterAdded))
begin
     set @isMultiSelectFilterSelected=1
end

declare @isSingleSelectFilterSelected bit=0
if(exists(select top 1 * from #singleSelectFilterAdded))
begin
     set @isSingleSelectFilterSelected=1
end



----------------------------------------------------------------------Num_Survey_DYnamic weekly daily-------------------------------------

/8create table #num_Surveys_Dynamics_weekly_daily(country_id nvarchar(1000),Resp_Static_ID nvarchar(1000),num_surveys bigint)	
insert into #num_Surveys_Dynamics_weekly_daily	
select  e.country_id,d.Resp_Static_ID,count(distinct d.Respondent_ID) from [dbo].[5 Demographics](nolock) a	
inner join #FilterWithoutBeverage b on a.index_resp=b.index_resp 	
inner join  [dbo].[Frequency Brand Source](nolock) d on a.Respondent_ID=d.Respondent_ID	
inner join Metrics e on d.Respondent_ID=e.Respondent_ID
group by e.country_id,d.Resp_Static_ID


-------------------------------------------------------------Benchmark Selection-------------------------------------------------------------

Drop Table if exists #BenchmarkFiltered
Create Table #BenchmarkFiltered ([index_country_drink_brand] bigint,categoryid int,[LowLevelCategoryID] int,[TrademarkID] bigint,[BrandID] bigint,Selectiontype nvarchar(500),SelectionID int,SelectionName Nvarchar(500),IscategoryorLowLevelcategory int, isConsumptionSelected bit,consumptionID int,consumptionName nvarchar(1000))
insert into #BenchmarkFiltered
	Select Distinct a.[index_country_drink_brand],a.categoryid,a.[LowLevelCategoryID],a.[TrademarkID],a.[BrandID],b.Selectiontype,b.selectionid,b.selectionname,(Case when b.BrandID is null then 1 else 0 end) IscategoryorLowLevelcategory, (
	case when b.ConsumptionID=1 then 0
	when b.ConsumptionID!=1 then 1
	end) isConsumptionSelected,b.ConsumptionID,
	(
	case when b.ConsumptionID=1 then 'Observed Drinkers'
	when b.ConsumptionID=2 then 'Daily'
	when b.ConsumptionID=3 then 'Weekly'
	when b.ConsumptionID=4 then 'Weekly+'
	when b.ConsumptionID=5 then 'Monthly'
	when b.ConsumptionID=6 then 'Occasional'
	end) consumptionName
	from DimProductBenchmarkMapping(nolock) a 
	inner join @BenchmarkComparisonSelections b on a.categoryid=b.CategoryID 
	and ((case when b.LowLevelCategoryID is not null then b.LowLevelCategoryID else 1 end)=(case when b.LowLevelCategoryID is not null then a.LowLevelCategoryID else 1 end))
	and ((case when b.TrademarkID is not null then b.TrademarkID else 1 end)=(case when b.TrademarkID is not null then a.TrademarkID else 1 end))
	and ((case when b.BrandID is not null then b.BrandID else 1 end)=(case when b.BrandID is not null then a.BrandID else 1 end))
	where (CountryID in (Select Distinct * from #country))



----------------------------------------------------Single Table---------------------------------------------------------------------
create table #singleSelect(index_resp bigint, index_country_drink_brand bigint)
insert into #singleSelect
select distinct b.index_resp,a.index_country_drink_brand  from [Diary - Single Select Questions](nolock) a
inner join #AddlFilter b on a.index_resp=b.index_resp
inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand



create table #consumptionSelected(country_id nvarchar(1000),index_resp bigint, index_country_drink_brand bigint,Brand_ID nvarchar(1000),Selectiontype nvarchar(1000),SelectionID int, SelectionName nvarchar(1000),IscategoryorLowLevelcategory int)
create table #consumptionSelectedEquity(index_resp bigint, index_country_drink_brand bigint,Brand_ID nvarchar(1000),Selectiontype nvarchar(1000),SelectionID int, SelectionName nvarchar(1000),IscategoryorLowLevelcategory int)
if @timeperiodSelected !=-1 and exists(select top 1 * from #BenchmarkFiltered where isConsumptionSelected=1)
begin

select distinct e.country_id,e.index_resp,a.index_country_drink_brand,d.Brand_ID,selectionType,SelectionID,SelectionName,IscategoryorLowLevelcategory into #consumptionData from [dbo].[Consumption_Mapping_Table1] a
inner join #countryData b on a.COuntryID=b.COuntryID and Timeperiod=@timeperiodSelected
inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand and c.isConsumptionSelected=1 and 
iif(c.consumptionID=2,a.Daily,1)=1 and iif(c.consumptionID=3,a.Weekly,1)=1 and iif(c.consumptionID=4,a.[Weekly+],1)=1 and iif(c.consumptionID=5,a.Monthly,1)=1 and iif(c.consumptionID=6,a.Occasional,1)=1
inner join [dbo].[Frequency Brand Source] d on a.index_country_drink_brand=d.index_country_drink_brand and a.Resp_Static_ID=d.Resp_Static_ID and b.country_ID=d.Country_ID
inner join [Metrics] e on d.Respondent_ID=e.Respondent_ID
inner join #AddlFilter f on e.index_resp=f.index_resp

insert into #consumptionSelected
select distinct a.country_id,a.index_resp,a.index_country_drink_brand,Brand_ID,Selectiontype,selectionID,SelectionName,IscategoryorLowLevelcategory  from #consumptionData a

insert into #consumptionSelected
select distinct e.country_id,a.index_resp,a.index_country_drink_brand,c.Brand_ID,b.Selectiontype,b.selectionID,b.SelectionName,IscategoryorLowLevelcategory 
from #singleSelect a inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand and b.isConsumptionSelected=0
inner join [4 Product] c on b.index_country_drink_brand=c.index_country_drink_brand
inner join [Metrics] e on a.index_resp=e.index_resp


insert into #consumptionSelectedEquity
select distinct a.index_resp,a.index_country_drink_brand,Brand_ID,Selectiontype,selectionID,SelectionName,IscategoryorLowLevelcategory  from #consumptionData a

insert into #consumptionSelectedEquity
select distinct d.index_resp,d.index_country_drink_brand,c.Brand_ID,b.Selectiontype,b.selectionID,b.SelectionName,IscategoryorLowLevelcategory 
from Equity(nolock) d
inner join #AddlFilterEquity a on d.index_resp=a.index_resp
inner join #BenchmarkFiltered b on d.index_country_drink_brand=b.index_country_drink_brand and b.isConsumptionSelected=0
inner join [4 Product] c on b.index_country_drink_brand=c.index_country_drink_brand
end
else
begin
insert into #consumptionSelected
select distinct e.country_id,a.index_resp,a.index_country_drink_brand,c.Brand_ID,b.Selectiontype,b.selectionID,b.SelectionName,IscategoryorLowLevelcategory 
from #singleSelect a inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand 
inner join [4 Product] c on b.index_country_drink_brand=c.index_country_drink_brand
inner join [Metrics] e on a.index_resp=e.index_resp

insert into #consumptionSelectedEquity
select distinct d.index_resp,d.index_country_drink_brand,c.Brand_ID,b.Selectiontype,b.selectionID,b.SelectionName,IscategoryorLowLevelcategory 
from Equity(nolock) d
inner join #AddlFilterEquity a on d.index_resp=a.index_resp
inner join #BenchmarkFiltered b on d.index_country_drink_brand=b.index_country_drink_brand 
inner join [4 Product] c on b.index_country_drink_brand=c.index_country_drink_brand
end




------------------------------------------------------------selection metric table-------------------------------------------------------------

create table #metric_selection(selectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000), consumptionID int,consumptionName nvarchar(1000))

insert into  #metric_selection
select b.SelectionType,b.selectionid,b.selectionName,a.MetricType,a.Metric,b.consumptionID,b.consumptionName
from
(
select distinct 'Activity' MetricType,AttributeName Metric  from dimAttributeName(nolock)
where AttributeGroup in (select AttributeGroupId from dimAttributeGroup where ColumnName='Activity')
union all
select distinct 'Activity_Group' MetricType,AttributeName Metric  from dimAttributeName(nolock)
where AttributeGroup in (select AttributeGroupId from dimAttributeGroup where ColumnName='Activity_Group')
union all
select distinct 'Reason' MetricType,AttributeName Metric  from dimAttributeName(nolock)
where AttributeGroup in (select AttributeGroupId from dimAttributeGroup where ColumnName='Reason') and AttributeName is not null
) a
cross join 
(
  select distinct SelectionType,SelectionID,SelectionName,consumptionID,consumptionName from #BenchmarkFiltered
) b

------------------------------------------------------------------------------------------------------------------------------------------
create table #metric_selection_Single(selectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),consumptionID int,consumptionName nvarchar(1000))

insert into  #metric_selection_Single
select b.SelectionType,b.selectionid,b.selectionName,a.MetricType,a.Metric,b.consumptionID,b.consumptionName
from
(
select distinct 'DayPart' MetricType,AttributeName Metric  from dimAttributeName(nolock)
where AttributeGroup in (select AttributeGroupId from dimAttributeGroup where ColumnName='DayPart_Name') and AttributeName is not null and AttributeName!='NA'
union all
select distinct 'Weekday/Weekend' MetricType,AttributeName Metric  from dimAttributeName(nolock)
where AttributeGroup in (select AttributeGroupId from dimAttributeGroup where ColumnName='Day_Name') and AttributeName is not null and AttributeName!='NA'
union all
select distinct 'Channel' MetricType,AttributeName Metric  from dimAttributeName(nolock)
where AttributeGroup in (select AttributeGroupId from dimAttributeGroup where ColumnName='Channel') and AttributeName is not null and AttributeName!='NA'
union all
select distinct 'Where_Group_HL' MetricType,AttributeName Metric  from dimAttributeName(nolock)
where AttributeGroup in (select AttributeGroupId from dimAttributeGroup where ColumnName='Where_Name') and AttributeName is not null and AttributeName!='NA'
union all
select distinct 'Where_Group_LL' MetricType,AttributeName Metric  from dimAttributeName(nolock)
where AttributeGroup in (select AttributeGroupId from dimAttributeGroup where ColumnName='Where_Away_Group') and AttributeName is not null and AttributeName!='NA'
union all
select distinct 'Occasions_Class' MetricType,AttributeName Metric  from dimAttributeNameocc(nolock)
where AttributeGroup in (select AttributeGroupId from dimAttributeGroupocc where ColumnName='Occasions_Class') and AttributeName is not null and AttributeName!='NA'
) a
cross join 
(
  select distinct SelectionType,SelectionID,SelectionName,consumptionID,consumptionName from #BenchmarkFiltered
) b


------------------------------------------------------------class drinker------------------------------------------------------------

create table #class_drinker(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),class_drinker int)

insert into  #class_drinker
select distinct SelectionType,selectionid,selectionName,metricType,metric,1 as class_drinker
from #metric_selection

------------------------------------------------------------------------------------------------------------------------------------------

create table #class_drinker_single(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),class_drinker int)

insert into #class_drinker_single
select distinct SelectionType,selectionid,selectionName,metricType,metric,1 as class_drinker
from #metric_selection_Single

------------------------------------------------------------------------------------------------------------------------------------------
create table #class_drinker_total(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),class_drinker int)

insert into #class_drinker_total
select distinct SelectionType,selectionid,selectionName,1 as class_drinker
from #metric_selection

------------------------------------------------------------Population-------------------------------------------------------------

declare @demogShare float = (select sum(weight) from Metrics a inner join #DemographicFilter b on a.index_resp=b.index_resp)
declare @totalShare float =  (select sum(weight) from Metrics a inner join #FilterTimeGeography b on a.index_resp=b.index_resp)

select Distinct Cast(Population as bigint) as population into #Populationlist from [5 Demographics] where country_ID in (select countryname from dimGeographyMapping a inner join #country b on a.countryID=b.countryID)


declare @totalPopulation float = (select Sum(cast(Population as bigint)) from #Populationlist)

declare @population float = (@demogShare/@totalShare)*@totalPopulation


create table #demogShare(country_id nvarchar(1000), weight float)
create table #totalShare(country_id nvarchar(1000), weight float)

insert into #demogShare
select a.country_id,sum(weight) weight from Metrics a inner join #DemographicFilter b on a.index_resp=b.index_resp group by a.country_id


insert into #totalShare
select a.country_id,sum(weight) weight from Metrics a inner join #FilterTimeGeography b on a.index_resp=b.index_resp group by a.country_id

create table #country_avg_population(country_id nvarchar(1000),Population float)
insert into #country_avg_population
select a.country_id,avg(Population) Population from Metrics a inner join #FilterTimeGeography b on a.index_resp=b.index_resp group by a.country_id

create table #country_population(country_id nvarchar(1000),Population float)
insert into #country_population
select a.country_id,(case when b.weight=0 then 0 else (a.weight/b.weight)*c.population end) population
from #demogShare a
inner join #totalShare b on a.country_id=b.country_id
inner join #country_avg_population c on b.country_id=c.country_id

declare @newPopulation float = (select sum(Population) from #country_population)


---------------------------------------------------------------Incidence Base Weighted----------------------------------------------

declare @incidence_base_weighted float = @demogShare

create table #incidence_base_weighted(country_id nvarchar(1000), weight float)
insert into #incidence_base_weighted
select country_id,weight from #demogShare


---------------------------------------------------------------Weighted Respondents------------------------------------------------

/8create table #weighted_Respondents(country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),weighted_respondent float)

if @isMultiSelectFilterSelected=1
begin
	insert into #weighted_Respondents
	select Country_id,SelectionType,selectionid,selectionName,metricType,Metric,sum(weight) weighted_respondent
	from
	(
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Activity' MetricType,a.Activity Metric,c.SelectionType,c.selectionid,c.selectionName   
	from #multiSelectFilterAdded a
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Activity_Group' MetricType,a.Activity_Group Metric,c.SelectionType,c.selectionid,c.selectionName   
	from #multiSelectFilterAdded a
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join Metrics d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Reason' MetricType,a.Reason Metric,c.SelectionType,c.selectionid,c.selectionName   
	from #multiSelectFilterAdded a
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	) a
	group by Country_id,MetricType,Metric,SelectionType,Selectionid,selectionName
end
else
begin
	
	insert into #weighted_Respondents
	select Country_id,SelectionType,selectionid,selectionName,metricType,Metric,sum(weight) weighted_respondent
	from
	(
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Activity' MetricType,a.Activity Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[MultiSelectTable](nolock) a
	inner join #consumptionSelected b on a.index_resp=b.index_resp and a.index_country_drink_brand=b.index_country_drink_brand
	inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Activity_Group' MetricType,a.Activity_Group Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[MultiSelectTable](nolock) a
	inner join #consumptionSelected b on a.index_resp=b.index_resp and a.index_country_drink_brand=b.index_country_drink_brand
	inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand
	inner join Metrics d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Reason' MetricType,a.Reason Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[MultiSelectTable](nolock) a
	inner join #consumptionSelected b on a.index_resp=b.index_resp and a.index_country_drink_brand=b.index_country_drink_brand
	inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	) a
	group by Country_id,MetricType,Metric,SelectionType,Selectionid,selectionName

	
end

------------------------------------------------------------------------------------------------------------------------------------------

create table #weighted_Respondents_single(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),weighted_respondent float)

if @isSingleSelectFilterSelected=1
begin
	insert into #weighted_Respondents_single
	select Country_id,SelectionType,selectionid,selectionName,metricType,Metric,sum(weight) weighted_respondent
	from
	(
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Daypart' MetricType,a.DayPart_Name Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Weekday/Weekend' MetricType,a.Day_Name Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join Metrics d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Channel' MetricType,a.Channel Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Where_Group_HL' MetricType,a.Where_Name Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Where_Group_LL' MetricType,a.Where_Away_Group Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	union all
    select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Occasions_Class' MetricType,a.Occasions_Class Metric,c.SelectionType,c.selectionid,c.selectionName  
    from [dbo].[Diary - Single Select Questions](nolock) a
    inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
    inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
    inner join Metrics(nolock) d on a.index_resp=d.index_resp
	) a
	group by Country_id,MetricType,Metric,SelectionType,Selectionid,selectionName
end
else
begin
	

	insert into #weighted_Respondents_single
	select Country_id,SelectionType,selectionid,selectionName,metricType,Metric,sum(weight) weighted_respondent
	from
	(
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Daypart' MetricType,a.DayPart_Name Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #consumptionSelected b on a.index_resp=b.index_resp and a.index_country_drink_brand=b.index_country_drink_brand
	inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Weekday/Weekend' MetricType,a.Day_Name Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #consumptionSelected b on a.index_resp=b.index_resp and a.index_country_drink_brand=b.index_country_drink_brand
	inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand
	inner join Metrics d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Channel' MetricType,a.Channel Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #consumptionSelected b on a.index_resp=b.index_resp and a.index_country_drink_brand=b.index_country_drink_brand
	inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Where_Group_HL' MetricType,a.Where_Name Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #consumptionSelected b on a.index_resp=b.index_resp and a.index_country_drink_brand=b.index_country_drink_brand
	inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	union all
	select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Where_Group_LL' MetricType,a.Where_Away_Group Metric,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #consumptionSelected b on a.index_resp=b.index_resp and a.index_country_drink_brand=b.index_country_drink_brand
	inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	Union all
    select distinct d.country_id,d.index_resp,cast(d.weight as float) weight,'Occasions_Class' MetricType,a.Occasions_Class Metric,c.SelectionType,c.selectionid,c.selectionName  
    from [dbo].[Diary - Single Select Questions](nolock) a
    inner join #consumptionSelected b on a.index_resp=b.index_resp and a.index_country_drink_brand=b.index_country_drink_brand
    inner join #BenchmarkFiltered c on a.index_country_drink_brand=c.index_country_drink_brand
    inner join Metrics(nolock) d on a.index_resp=d.index_resp
	) a
	group by Country_id,MetricType,Metric,SelectionType,Selectionid,selectionName
	
end

------------------------------------------------------------------------------------------------------------------------------------------

create table #weighted_Respondents_total(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),weighted_respondent float)
if @isSingleSelectFilterSelected=1
begin 
	insert into #weighted_Respondents_total
	select Country_id,SelectionType,selectionid,selectionName,sum(weight) weighted_respondent
	from
	(
	select distinct d.Country_id,d.index_resp,cast(d.weight as float) weight,c.SelectionType,c.selectionid,c.selectionName   
	from [dbo].[Diary - Single Select Questions](nolock) a
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join Metrics(nolock) d on a.index_resp=d.index_resp
	) a
	group by Country_id,SelectionType,Selectionid,selectionName
end
else 
begin 
	insert into #weighted_Respondents_total
	select Country_id,SelectionType,selectionid,selectionName,sum(weight) weighted_respondent
	from
	(
	select distinct d.Country_id,d.index_resp,cast(d.weight as float) weight,c.SelectionType,c.selectionid,c.selectionName   
	from #consumptionSelected c 
	inner join Metrics(nolock) d on c.index_resp=d.index_resp
	) a
	group by Country_id,SelectionType,Selectionid,selectionName

end

---------------------------------------------------------------Observer drinker Weighted---------------------------------------------------------
create table #observe_drinker_weighted(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),observeDrinkerWeighted float)

insert into #observe_drinker_weighted
select a.Country_id,a.SelectionType,a.selectionid,a.selectionName,a.metricType,a.Metric,class_drinker*weighted_respondent
from  #weighted_Respondents a
inner join #class_drinker b on a.SelectionType=b.SelectionType and a.selectionid=b.selectionid and a.selectionName=b.selectionName and a.metricType=b.metricType and a.Metric=b.Metric

------------------------------------------------------------------------------------------------------------------------------------------

create table #observe_drinker_weighted_single(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),observeDrinkerWeighted float)

insert into #observe_drinker_weighted_single
select a.Country_id,a.SelectionType,a.selectionid,a.selectionName,a.metricType,a.Metric,class_drinker*weighted_respondent
from  #weighted_Respondents_single a
inner join #class_drinker_single b on a.SelectionType=b.SelectionType and a.selectionid=b.selectionid and a.selectionName=b.selectionName and a.metricType=b.metricType and a.Metric=b.Metric

------------------------------------------------------------------------------------------------------------------------------------------

create table #observe_drinker_weighted_total(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),observeDrinkerWeighted float)

insert into #observe_drinker_weighted_total
select a.Country_id,a.SelectionType,a.selectionid,a.selectionName,class_drinker*weighted_respondent
from  #weighted_Respondents_total a
inner join #class_drinker_total b on a.SelectionType=b.SelectionType and a.selectionid=b.selectionid and a.selectionName=b.selectionName 

------------------------------------------------------------------------------------------------------------------------------------------

create table #observe_drinker_weighted_weeklyPlus(country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),observeDrinkerWeighted float)

create table #observe_drinker_weighted_daily(country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),observeDrinkerWeighted float)

  select country_id,Resp_Static_ID,SelectionType,selectionid,selectionName,sum(wp_week) wp_week,sum(weight) weight into #wp_week_weekly_daily
   from
   (
		select country_id,Resp_Static_ID,Respondent_ID,SelectionType,selectionid,selectionName,max(Freq_Numeric) as wp_week,max(weight) weight from 
		(
		select distinct e.country_id,d.Resp_Static_ID,d.Respondent_ID,d.Freq_Numeric,c.SelectionType,c.selectionid,c.selectionName,d.weight,d.num_surveys 
		from [dbo].[Diary - Single Select Questions](nolock) a
		inner join #consumptionSelected c on a.index_resp=c.index_resp and a.index_country_drink_brand=c.index_country_drink_brand
		inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand
		inner join [dbo].[Frequency Brand Source](nolock) d on c.index_country_drink_brand=d.index_country_drink_brand and a.index_resp_drink_brand=d.index_resp_drink_brand
		inner join Metrics e on d.Respondent_ID=e.Respondent_ID
		) a 
		group by country_id,Resp_Static_ID,Respondent_ID,SelectionType,selectionid,selectionName 
   ) a
   group by country_id,Resp_Static_ID,SelectionType,selectionid,selectionName



   insert into #observe_drinker_weighted_weeklyPlus
   select a.country_id,SelectionType,selectionid,selectionName,sum(weight) as observeDrinkerWeighted
   from  #wp_week_weekly_daily a
   inner join #num_Surveys_Dynamics_weekly_daily b
   on a.Resp_Static_ID=b.Resp_Static_ID and a.country_id=b.country_id
   where (wp_week/num_surveys)>=3
   group by a.country_id,SelectionType,selectionid,selectionName 

   insert into  #observe_drinker_weighted_daily
   select a.country_id,SelectionType,selectionid,selectionName,sum(weight) as observeDrinkerWeighted
   from  #wp_week_weekly_daily a
   inner join #num_Surveys_Dynamics_weekly_daily b
   on a.Resp_Static_ID=b.Resp_Static_ID and a.country_id=b.country_id
   where (wp_week/num_surveys)>=4
   group by a.country_id,SelectionType,selectionid,selectionName 

---------------------------------------------------------------Observer drinker Population---------------------------------------------------------
create table #observe_drinker_pop(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),observeDrinkerPopulation float)

insert into #observe_drinker_pop
select a.country_id,SelectionType,selectionid,selectionName,metricType,Metric,(observeDrinkerWeighted /b.weight)*c.population
from  #observe_drinker_weighted a
inner join #incidence_base_weighted b on a.country_id=b.country_id
inner join #country_population c on b.country_id=c.country_id

------------------------------------------------------------------------------------------------------------------------------------------

create table #observe_drinker_pop_single(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),observeDrinkerPopulation float)

insert into #observe_drinker_pop_single
select a.country_id,SelectionType,selectionid,selectionName,metricType,Metric,(observeDrinkerWeighted /b.weight)*c.population
from  #observe_drinker_weighted_single a
inner join #incidence_base_weighted b on a.country_id=b.country_id
inner join #country_population c on b.country_id=c.country_id

------------------------------------------------------------------------------------------------------------------------------------------

create table #observe_drinker_pop_total(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),observeDrinkerPopulation float)

insert into #observe_drinker_pop_total
select a.country_id,SelectionType,selectionid,selectionName,(observeDrinkerWeighted /b.weight)*c.population
from  #observe_drinker_weighted_total a
inner join #incidence_base_weighted b on a.country_id=b.country_id
inner join #country_population c on b.country_id=c.country_id


------------------------------------------------------------------------------------------------------------------------------------------

create table #observe_drinker_pop_weeklyPlus(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),observeDrinkerPopulation float)

insert into #observe_drinker_pop_weeklyPlus
select SelectionType,selectionid,selectionName,sum(population) 
from
(
select SelectionType,selectionid,selectionName,(observeDrinkerWeighted /b.weight)*c.population as population
from  #observe_drinker_weighted_weeklyPlus a
inner join #incidence_base_weighted b on a.country_id=b.country_id
inner join #country_population c on b.country_id=c.country_id
) a
group by SelectionType,selectionid,selectionName

------------------------------------------------------------------------------------------------------------------------------------------

create table #observe_drinker_pop_daily(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),observeDrinkerPopulation float)

insert into #observe_drinker_pop_daily
select SelectionType,selectionid,selectionName,sum(population) 
from
(
select SelectionType,selectionid,selectionName,(observeDrinkerWeighted /b.weight)*c.population as population
from  #observe_drinker_weighted_daily a
inner join #incidence_base_weighted b on a.country_id=b.country_id
inner join #country_population c on b.country_id=c.country_id
) a
group by SelectionType,selectionid,selectionName


---------------------------------------------------------------Weekly+ and Daily Percentage-----------------------------------------------

create table #weighted_weeklyPlus(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),weight float)
insert into #weighted_weeklyPlus
select SelectionType,selectionid,selectionName,observeDrinkerPopulation/@newPopulation from  #observe_drinker_pop_weeklyPlus a

------------------------------------------------------------------------------------------------------------------------------------------

create table #weighted_daily(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),weight float)
insert into #weighted_daily
select SelectionType,selectionid,selectionName,observeDrinkerPopulation/@newPopulation from  #observe_drinker_pop_daily a

---------------------------------------------------------------Reported Drinks-----------------------------------------------------------
/8create table #reported_drink(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),reportedDrink float)

if @isMultiSelectFilterSelected=1
begin
	insert into #reported_drink
	select country_id,SelectionType,selectionid,selectionName,metricType,Metric,sum(weight*drinks) reportedDrink
	from
	(
	select distinct c.country_id,SelectionType,selectionid,selectionName,'Activity' metricType,a.Activity Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  #multiSelectFilterAdded a 
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	union all
	select distinct c.country_id,SelectionType,selectionid,selectionName,'Activity_Group' metricType,a.Activity_Group Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  #multiSelectFilterAdded a 
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	union all
	select distinct c.country_id,SelectionType,selectionid,selectionName,'Reason' metricType,a.Reason Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  #multiSelectFilterAdded a 
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	) a
	group by country_id,SelectionType,selectionid,selectionName,metricType,Metric
end
else
begin 
	insert into #reported_drink
	select country_id,SelectionType,selectionid,selectionName,metricType,Metric,sum(weight*drinks) reportedDrink
	from
	(
	select distinct c.country_id,c.SelectionType,c.selectionid,c.selectionName,'Activity' metricType,a.Activity Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[MultiSelectTable](nolock) a  
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand
	union all
	select distinct c.country_id,c.SelectionType,c.selectionid,c.selectionName,'Activity_Group' metricType,a.Activity_Group Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[MultiSelectTable](nolock) a  
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand
	union all
	select distinct c.country_id,c.SelectionType,c.selectionid,c.selectionName,'Reason' metricType,a.Reason Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[MultiSelectTable](nolock) a  
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand
	) a
	group by country_id,SelectionType,selectionid,selectionName,metricType,Metric
end

------------------------------------------------------------------------------------------------------------------------------------------

create table #reported_drink_single(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),reportedDrink float)

if @isSingleSelectFilterSelected=1
begin
	insert into #reported_drink_single
	select country_id,SelectionType,selectionid,selectionName,metricType,Metric,sum(weight*drinks) reportedDrink
	from
	(
	select distinct c.country_id,SelectionType,selectionid,selectionName,'Daypart' metricType,a.DayPart_Name Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	union all
	select distinct c.country_id,SelectionType,selectionid,selectionName,'Weekday/Weekend' metricType,a.Day_Name Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	union all
	select distinct c.country_id,SelectionType,selectionid,selectionName,'Channel' metricType,a.Channel Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	union all
	select distinct c.country_id,SelectionType,selectionid,selectionName,'Where_Group_HL' metricType,a.Where_Name Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	union all
	select distinct c.country_id,SelectionType,selectionid,selectionName,'Where_Group_LL' metricType,a.Where_Away_Group Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	union all
    select distinct c.country_id,SelectionType,selectionid,selectionName,'Occasions_Class' metricType,a.Occasions_Class Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
    from  [dbo].[Diary - Single Select Questions](nolock) a
    inner join #singleSelectFilterAdded e on a.index_resp_iterator=e.index_resp_iterator
    inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	) a
	group by country_id,SelectionType,selectionid,selectionName,metricType,Metric
end
else
begin
	insert into #reported_drink_single
	select country_id,SelectionType,selectionid,selectionName,metricType,Metric,sum(weight*drinks) reportedDrink
	from
	(
	select distinct c.country_id,c.SelectionType,c.selectionid,c.selectionName,'Daypart' metricType,a.DayPart_Name Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand
	union all
	select distinct c.country_id,c.SelectionType,c.selectionid,c.selectionName,'Weekday/Weekend' metricType,a.Day_Name Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand
	union all
	select distinct c.country_id,c.SelectionType,c.selectionid,c.selectionName,'Channel' metricType,a.Channel Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand
	union all
	select distinct c.country_id,c.SelectionType,c.selectionid,c.selectionName,'Where_Group_HL' metricType,a.Where_Name Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand
	union all
	select distinct c.country_id,c.SelectionType,c.selectionid,c.selectionName,'Where_Group_LL' metricType,a.Where_Away_Group Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand
	union all
	select distinct c.country_id,c.SelectionType,c.selectionid,c.selectionName,'Occasions_Class' metricType,a.Occasions_Class Metric,a.index_resp_iterator,cast(a.weight as float) weight,a.drinks
	from  [dbo].[Diary - Single Select Questions](nolock) a 
	inner join #consumptionSelected c on a.index_country_drink_brand=c.index_country_drink_brand and a.index_resp=c.index_resp
	inner join #BenchmarkFiltered b on a.index_country_drink_brand=b.index_country_drink_brand
	) a
	group by country_id,SelectionType,selectionid,selectionName,metricType,Metric
end

------------------------------------------------------------------------------------------------------------------------------------------

create table #reported_drink_total(Country_id nvarchar(1000),SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),reportedDrink float)

if @isSingleSelectFilterSelected=1
begin
	insert into #reported_drink_total
	select country_id,SelectionType,selectionid,selectionName,sum(weight*drinks) reportedDrink
	from
	(
	select distinct b.country_id,SelectionType,selectionid,selectionName,d.index_resp_iterator,cast(d.weight as float) weight,cast(d.drinks as float) drinks
	from  #consumptionSelected b 
	inner join [dbo].[Diary - Single Select Questions](nolock) d on b.index_resp=d.index_resp and b.index_country_drink_brand=d.index_country_drink_brand
	inner join #singleSelectFilterAdded e on d.index_resp_iterator=e.index_resp_iterator
	) a
	group by country_id,SelectionType,selectionid,selectionName
end
else 
begin
	insert into #reported_drink_total
	select country_id,SelectionType,selectionid,selectionName,sum(weight*drinks) reportedDrink
	from
	(
	select distinct b.country_id,SelectionType,selectionid,selectionName,d.index_resp_iterator,cast(d.weight as float) weight,cast(d.drinks as float) drinks
	from  #consumptionSelected b 
	inner join [dbo].[Diary - Single Select Questions](nolock) d on b.index_resp=d.index_resp and b.index_country_drink_brand=d.index_country_drink_brand
	) a
	group by country_id,SelectionType,selectionid,selectionName
end


-----------------------------------------------------------------Sample size table-------------------------------------------------------------

/8create table #sample_size(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),samplesize bigint)

insert into #sample_size
select SelectionType,selectionid,selectionName,count(distinct a.index_resp) samplesize
from #consumptionSelected a 
group by SelectionType,selectionid,selectionName

-----------------------------------------------------------------Total Drinks-------------------------------------------------------------


declare @total_days int = (select sum(Days_In_Month) from [dbo].[3 Period (Monthly)] a inner join #timeperiod b on a.Month_ID=b.Timeperiod)
declare @num_weeks float = (cast(@total_days as float)/7)

create table #total_drinker(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),totalDrinker float)
insert into #total_drinker

select SelectionType,selectionid,selectionName,metricType,Metric,sum(TotalDrinker) TotalDrinker
from
(
select a.country_id,a.SelectionType,a.selectionid,a.selectionName,a.metricType,a.Metric,(b.ReportedDrink/a.weighted_respondent)*c.observeDrinkerPopulation*@num_weeks TotalDrinker
from  #weighted_Respondents a
inner join #reported_drink b on a.SelectionType=b.SelectionType and a.selectionid=b.selectionid and a.selectionName=b.selectionName and a.MetricType=b.MetricType and a.Metric=b.Metric and a.country_id=b.country_id
inner join #observe_drinker_pop c on c.SelectionType=b.SelectionType and c.selectionid=b.selectionid and c.selectionName=b.selectionName and c.MetricType=b.MetricType and c.Metric=b.Metric and c.country_id=b.country_id
) a
group by SelectionType,selectionid,selectionName,metricType,Metric

------------------------------------------------------------------------------------------------------------------------------------------

create table #total_drinker_single(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),totalDrinker float)
insert into #total_drinker_single

select SelectionType,selectionid,selectionName,metricType,Metric,sum(TotalDrinker) TotalDrinker
from
(
select a.country_id,a.SelectionType,a.selectionid,a.selectionName,a.metricType,a.Metric,(b.ReportedDrink/a.weighted_respondent)*c.observeDrinkerPopulation*@num_weeks TotalDrinker
from  #weighted_Respondents_single a
inner join #reported_drink_single b on a.SelectionType=b.SelectionType and a.selectionid=b.selectionid and a.selectionName=b.selectionName and a.MetricType=b.MetricType and a.Metric=b.Metric and a.country_id=b.country_id
inner join #observe_drinker_pop_single c on c.SelectionType=b.SelectionType and c.selectionid=b.selectionid and c.selectionName=b.selectionName and c.MetricType=b.MetricType and c.Metric=b.Metric and c.country_id=b.country_id
) a
group by SelectionType,selectionid,selectionName,metricType,Metric

------------------------------------------------------------------------------------------------------------------------------------------

create table #total_drinker_total(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),totalDrinker float)
insert into #total_drinker_total

select SelectionType,selectionid,selectionName,sum(TotalDrinker) TotalDrinker
from
(
select a.country_id,a.SelectionType,a.selectionid,a.selectionName,(b.ReportedDrink/a.weighted_respondent)*c.observeDrinkerPopulation*@num_weeks TotalDrinker
from  #weighted_Respondents_total a
inner join #reported_drink_total b on a.SelectionType=b.SelectionType and a.selectionid=b.selectionid and a.selectionName=b.selectionName and a.country_id=b.country_id
inner join #observe_drinker_pop_total c on c.SelectionType=b.SelectionType and c.selectionid=b.selectionid and c.selectionName=b.selectionName and c.country_id=b.country_id
) a
group by SelectionType,selectionid,selectionName

----------------------------------------------------------------weekly+ and daily Significance Calculation-------------------------------------

create table #weekly_daily_intermediate(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000), metrictype nvarchar(100), metric nvarchar(1000), percentage float,baseSampleSize bigint)

insert into #weekly_daily_intermediate
select a.SelectionType,a.selectionId,a.selectionName,'Consumption' metricType, 'Weekly+' metric,a.weight as Percentage,b.samplesize
from #weighted_weeklyPlus a
inner join #sample_size b on a.selectionType=b.selectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName
union all
select a.SelectionType,a.selectionId,a.selectionName,'Consumption' metricType, 'Daily' metric,a.weight as Percentage,b.samplesize
from #weighted_daily a
inner join #sample_size b on a.selectionType=b.selectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName

------------------------------------------------------------------------------------------------------------------------------------------

create table #weekly_daily_output(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metrictype nvarchar(100), metric nvarchar(1000),percentage float,significance float,slideNumber int)

insert into #weekly_daily_output
select a.SelectionType,a.selectionId,a.selectionName,a.metricType,a.metric,a.Percentage,0 significance,13 as Slidenumber
from #weekly_daily_intermediate a
where a.selectionType='Benchmark'

insert into #weekly_daily_output
select a.SelectionType,a.selectionId,a.selectionName,a.metricType,a.metric,a.Percentage,[dbo].[significanceValue](b.Percentage,b.basesamplesize,a.Percentage,a.basesamplesize) significance,13 as Slidenumber
from #weekly_daily_intermediate a
inner join #weekly_daily_intermediate b
on a.selectionType!='Benchmark' and b.selectiontype='Benchmark' and a.metrictype=b.metrictype and a.metric=b.metric


insert into #weekly_daily_output
select SelectionType,selectionid,selectionName,'Consumption' metricType,'Weekly+(Obs. Pop.)' metric,observeDrinkerPopulation,0 significance,13 as Slidenumber  
from #observe_drinker_pop_weeklyPlus

insert into #weekly_daily_output
select SelectionType,selectionid,selectionName,'Consumption' metricType,'Daily+(Obs. Pop.)' metric,observeDrinkerPopulation,0 significance,13 as Slidenumber  
from #observe_drinker_pop_daily


----------------------------------------------------------------Equity table numerator----------------------------------------------------------


/8create table #equity_numerator(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),weight float,slidenumber int,IscategoryorLowLevelcategory int)


select distinct index_resp,Brand_ID,Selectiontype,selectionID,SelectionName,IscategoryorLowLevelcategory into #consumptionSelectedNew
from #consumptionSelectedEquity



select a.index_resp, Aware, Aware_Fup, Fam_ID, Cons_ID, Last_Ten, Blast, Affinity_ID ,Unique_ID	,MNeeds_ID	,Dynamic_ID	,Price_ID	,Worth_ID	,Fam_Aware	,Fam_Tried	,Weight,Selectiontype,selectionID,SelectionName,IscategoryorLowLevelcategory into #equity
from #consumptionSelectedNew c
inner join [Equity](nolock) a on a.index_resp=c.index_resp and a.Brand_ID=c.Brand_ID

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Equity' metricType, 'Affinity' metric,Sum(cast(a.weight as float)) weight,12 as slidenumber,a.IscategoryorLowLevelcategory
from #equity a
where a.Affinity_ID>=6  and a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Equity' metricType, 'Uniqueness' metric,Sum(cast(a.weight as float)) weight,12 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Unique_ID>=6 and a.Fam_Aware=1 
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Equity' metricType, 'Meets Needs' metric,Sum(cast(a.weight as float)) weight,12 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.MNeeds_ID>=6 and a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Equity' metricType, 'Dynamic' metric,Sum(cast(a.weight as float)) weight,12 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Dynamic_ID>=6 and a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Equity' metricType, 'Price' metric,Sum(cast(a.weight as float)) weight,12 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Price_ID>=6 and a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Equity' metricType, 'Sets Trends' metric,Sum(cast(a.weight as float)) weight,12 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Dynamic_ID>=6 and a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Equity' metricType, 'Consideration' metric,Sum(cast(a.weight as float)) weight,12 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Cons_ID=4 and a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Equity' metricType, 'Worth More' metric,Sum(cast(a.weight as float)) weight,12 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Worth_ID=1 and a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Equity' metricType, 'Worth Same' metric,Sum(cast(a.weight as float)) weight,12 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Worth_ID=2 and a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Equity' metricType, 'Worth Less' metric,Sum(cast(a.weight as float)) weight,12 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Worth_ID=3 and a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Awareness' metricType, 'Top of Mind' metric,Sum(cast(a.weight as float)) weight,13 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Awareness' metricType, 'Spontaneous' metric,Sum(cast(a.weight as float)) weight,13 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where (a.Aware>0 or a.Aware_Fup>0)
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Awareness' metricType, 'Total' metric,Sum(cast(a.weight as float)) weight,13 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

insert into #equity_numerator
select a.selectionType,a.selectionId,a.selectionName, 'Familiarity' metricType, 'Tried' metric,Sum(cast(a.weight as float)) weight,13 as slidenumber,a.IscategoryorLowLevelcategory 
from #equity a
where a.Fam_Tried=1
group by a.selectionType,a.selectionId,a.selectionName,a.IscategoryorLowLevelcategory

update #equity_numerator set weight=0 where IscategoryorLowLevelcategory=1

----------------------------------------------------------------Equity table denominator--------------------------------------------------------

create table #equity_denominator(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),weight float,samplesize bigint)
insert into #equity_denominator
select a.selectionType,a.selectionId,a.selectionName, sum(cast(a.weight as float)) weight, count(distinct a.index_resp) samplesize
from #equity a
group by a.selectionType,a.selectionId,a.selectionName


create table #equity_denominator_new(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),weight float,samplesize bigint)
insert into #equity_denominator_new
select a.selectionType,a.selectionId,a.selectionName, sum(cast(a.weight as float)) weight, count(distinct a.index_resp) samplesize
from #equity a
where a.Fam_Aware=1
group by a.selectionType,a.selectionId,a.selectionName

----------------------------------------------------------------Equity table Output-----------------------------------------------------------
create table #equity_output_intermediate(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),percentage float,slidenumber int, basesamplesize bigint)

insert into #equity_output_intermediate
select a.selectionType,a.selectionId,a.selectionName,a.metricType,a.metric,isnull(isnull(a.weight,0)/isnull(b.weight,1),0) as percentage,a.slidenumber,b.samplesize
from #equity_numerator a
inner join #equity_denominator b on a.slideNumber=13 and a.selectionType=b.selectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName

insert into #equity_output_intermediate
select a.selectionType,a.selectionId,a.selectionName,a.metricType,a.metric,isnull(isnull(a.weight,0)/isnull(b.weight,1),0) as percentage,a.slidenumber,b.samplesize
from #equity_numerator a
inner join #equity_denominator_new b on a.slideNumber=12 and a.selectionType=b.selectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName



----------------------------------------------------------------Equity table Output-----------------------------------------------------------
create table #equity_output_result(SelectionType nvarchar(1000),selectionId bigint,selectionName nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),percentage float,slidenumber int, significance float)

insert into #equity_output_result
select a.selectionType,a.selectionId,a.selectionName,a.metricType,a.metric,a.percentage,a.slidenumber,0 significance
from #equity_output_intermediate a
where a.selectionType='Benchmark'

Declare @bench int = (Select Count(*) from #equity_output_result)

If (@bench=0)
begin
insert into #equity_output_result
select a.selectionType,a.selectionId,a.selectionName,a.metricType,a.metric,a.percentage,a.slidenumber,[dbo].[significanceValue](isnull(b.Percentage,0),isnull(b.basesamplesize,0),a.Percentage,a.basesamplesize) significance
from #equity_output_intermediate a
left join #equity_output_intermediate b 
on a.slidenumber=b.slidenumber and a.selectionType!='Benchmark' and b.selectiontype='Benchmark' and a.metrictype=b.metrictype and a.metric=b.metric
end
else
begin
insert into #equity_output_result
select a.selectionType,a.selectionId,a.selectionName,a.metricType,a.metric,a.percentage,a.slidenumber,[dbo].[significanceValue](isnull(b.Percentage,0),isnull(b.basesamplesize,0),a.Percentage,a.basesamplesize) significance
from #equity_output_intermediate a
left join #equity_output_intermediate b 
on a.slidenumber=b.slidenumber and a.selectionType!='Benchmark' and b.selectiontype='Benchmark' and a.metrictype=b.metrictype and a.metric=b.metric
where a.slidenumber=b.slidenumber and a.selectionType!='Benchmark' and b.selectiontype='Benchmark' and a.metrictype=b.metrictype and a.metric=b.metric
end
---------------------------------------------------------------Output Equity and Consumption--------------------------------------------------

create table #template_equity_consumption(selectionType nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),selectionId int,selectionName nvarchar(1000),groupSort int,sortid int,slideNumber int, isCategory int,consumptionID int,consumptionName nvarchar(1000))

insert into  #template_equity_consumption
select distinct a.selectionType,b.AttributeGroupName metricType,b.AttributeName metric,a.selectionId,a.selectionName,b.attributeGroupSortId,b.AttributeSortId,b.slideNumber,a.IscategoryorLowLevelcategory isCategory,a.consumptionID consumptionID,a.consumptionName
from #BenchmarkFiltered a
cross join Mapping_Metric_PPT(nolock) b
where b.SlideNumber in (12,13)

create table #output_equity_consumption(selectionType nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),selectionId int,selectionName nvarchar(1000),percentage DECIMAL(15,4),significance DECIMAL(10,4),groupSort int,sortid int,slideNumber int, isCategory int, consumptionID int,consumptionName nvarchar(1000))

insert into  #output_equity_consumption
select a.selectionType,a.metricType,a.metric,a.selectionId,a.selectionName,CAST(ROUND(isnull(b.percentage,0), 4) AS DECIMAL(10,4)) percentage,CAST(ROUND(isnull(b.significance,0), 4) AS DECIMAL(10,4)) significance,a.groupsort,a.sortid,a.slidenumber ,a.isCategory,a.consumptionID,a.consumptionName
from #template_equity_consumption a 
left join #equity_output_result b
on a.slidenumber=b.slidenumber and a.selectionType=b.selectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName and a.metricType=b.metrictype and a.metric=b.metric
where a.metrictype!='Consumption'

insert into  #output_equity_consumption
select a.selectionType,a.metricType,a.metric,a.selectionId,a.selectionName,CAST(ROUND(isnull(b.percentage,0), 4) AS DECIMAL(15,4)) percentage,CAST(ROUND(isnull(b.significance,0), 4) AS DECIMAL(10,4)) significance,a.groupsort,a.sortid,a.slidenumber  ,a.isCategory,a.consumptionID,a.consumptionName
from #template_equity_consumption a 
left join #weekly_daily_output b
on a.slidenumber=b.slidenumber and a.selectionType=b.selectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName and a.metricType=b.metrictype and a.metric=b.metric
where a.metrictype='Consumption'

--------------------------------------------------------------------------------------------------------------------------------------------

create table #slideTemplate_single(metricType nvarchar(1000),metric nvarchar(1000),AttributeName nvarchar(1000),slideNumber int,GroupSort int, sortid int)

insert into  #slideTemplate_single
select distinct a.AttributeGroupName metricType,b.AttributeName metric,a.AttributeName,a.slidenumber,a.AttributeGroupSortID GroupSort,a.AttributeSortId sortId 
from Mapping_Metric_PPT(nolock) a
inner join [dbo].[DimAttributeName] b on a.AttributeGroupId=b.AttributeGroup and a.AttributeId=b.AttributeID and a.slidenumber in (8,9,11,15) and not(@isIndiaSelected=0 and SlideNumber=15 and a.AttributeGroupID=45 and a.AttributeID=1)


create table #slideTemplate(metricType nvarchar(1000),metric nvarchar(1000),slideNumber int,GroupSort int, sortid int)

insert into  #slideTemplate
select distinct MetricType,Metric,6 as SlideNumber,1 as groupSortID,rowNum sortId
from
(
	select a.SelectionType,a.MetricType,a.Metric,a.SelectionId,a.SelectionName,isnull(b.Percentage,0) Percentage, c.AttributeSortID rowNum --,1 as groupSort,row_number() over(order by (select 1)) sortId,7 as SlideNumber
	from #metric_selection a
	inner join Mapping_Metric_PPT c on a.Metric=c.AttributeName and c.SlideNumber=6
	left join
	(
	select a.SelectionType,a.MetricType,a.Metric,a.SelectionId,a.SelectionName,CAST(ROUND(isnull((a.totalDrinker/b.totalDrinker),0), 4) AS DECIMAL(10,4)) as Percentage
	from #total_drinker a
	inner join  #total_drinker_total b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName
	) b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName and a.MetricType=b.MetricType and a.Metric=b.Metric
	where a.MetricType='Activity_Group'  and a.SelectionType='Benchmark'
) a
where rowNum<=9

insert into  #slideTemplate
select distinct MetricType,Metric,7 as SlideNumber,1 as groupSortID,rowNum sortId
from
(
	select a.SelectionType,a.MetricType,a.Metric,a.SelectionId,a.SelectionName,isnull(b.Percentage,0) Percentage, row_number() over(partition by a.SelectionType,a.SelectionId,a.SelectionName order by isnull(b.Percentage,0) desc ) rowNum --,1 as groupSort,row_number() over(order by (select 1)) sortId,7 as SlideNumber
	from #metric_selection a
	left join
	(
	select a.SelectionType,a.MetricType,a.Metric,a.SelectionId,a.SelectionName,CAST(ROUND(isnull((a.totalDrinker/b.totalDrinker),0), 4) AS DECIMAL(10,4)) as Percentage
	from #total_drinker a
	inner join  #total_drinker_total b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName
	) b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName and a.MetricType=b.MetricType and a.Metric=b.Metric
	where a.MetricType='Activity'  and a.SelectionType='Benchmark'
) a
where rowNum<=10

insert into  #slideTemplate
select distinct MetricType,Metric,10 as SlideNumber,1 as groupSortID,rowNum sortId
from
(
	select a.SelectionType,a.MetricType,a.Metric,a.SelectionId,a.SelectionName,isnull(b.Percentage,0) Percentage, row_number() over(partition by a.SelectionType,a.SelectionId,a.SelectionName order by isnull(b.Percentage,0) desc ) rowNum --,1 as groupSort,row_number() over(order by (select 1)) sortId,8 as SlideNumber
	from #metric_selection a
	left join
	(
	select a.SelectionType,a.MetricType,a.Metric,a.SelectionId,a.SelectionName,CAST(ROUND(isnull((a.totalDrinker/b.totalDrinker),0), 4) AS DECIMAL(10,4)) as Percentage
	from #total_drinker a
	inner join  #total_drinker_total b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName
	) b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName  and a.MetricType=b.MetricType and a.Metric=b.Metric
	where a.MetricType='Reason'  and a.SelectionType='Benchmark'
) a
where rowNum<=10

------------------------------------------------------------------------------------------------------------------------------------------

create table #finalOutputIntermediate(selectionType nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),selectionId int,selectionName nvarchar(1000),percentage DECIMAL(10,4),significance DECIMAL(10,4),groupSort int,sortid int,slideNumber int,consumptionID int,consumptionName nvarchar(1000))


insert into #finalOutputIntermediate
select a.SelectionType,a.MetricType,a.Metric,a.SelectionId,a.SelectionName,isnull(b.Percentage,0) Percentage,isnull(b.significance,0) Significance, c.groupSort,c.sortId,c.SlideNumber SlideNumber,a.consumptionID,a.consumptionName
from #metric_selection a
inner join  #slideTemplate c on a.MetricType=c.MetricType and a.Metric=c.Metric
left join
(
select a.SelectionType,a.MetricType,a.Metric,a.SelectionId,a.SelectionName,CAST(ROUND(isnull((a.totalDrinker/b.totalDrinker),0), 4) AS DECIMAL(10,4)) as Percentage,0 as Significance
from #total_drinker a
inner join  #total_drinker_total b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName
) b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName and a.MetricType=b.MetricType and a.Metric=b.Metric

------------------------------------------------------------------------------------------------------------------------------------------

create table #finalOutputIntermediate_Single(selectionType nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),attributeName nvarchar(1000),selectionId int,selectionName nvarchar(1000),percentage DECIMAL(10,4),significance DECIMAL(10,4),groupSort int,sortid int,slideNumber int,consumptionID int,consumptionName nvarchar(1000))

insert into #finalOutputIntermediate_Single
select a.SelectionType,a.MetricType,a.Metric,c.AttributeName,a.SelectionId,a.SelectionName,isnull(b.Percentage,0) Percentage,isnull(b.significance,0) Significance, c.groupSort,c.sortId,c.SlideNumber SlideNumber,a.consumptionID,a.consumptionName
from #metric_selection_Single a
inner join  #slideTemplate_Single c on a.MetricType=c.MetricType and a.Metric=c.Metric
left join
(
select a.SelectionType,a.MetricType,a.Metric,a.SelectionId,a.SelectionName,CAST(ROUND(isnull((a.totalDrinker/b.totalDrinker),0), 4) AS DECIMAL(10,4)) as Percentage,0 as Significance
from #total_drinker_single a
inner join  #total_drinker_total b
on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName
) b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName and a.MetricType=b.MetricType and a.Metric=b.Metric

------------------------------------------------------------------------------------------------------------------------------------------

create table #Output(selectionType nvarchar(1000),metricType nvarchar(1000),metric nvarchar(1000),selectionId int,selectionName nvarchar(1000),percentage DECIMAL(10,4),significance DECIMAL(10,4),groupSort int,sortid int,slideNumber int,isCategory int,consumptionID int,consumptionName nvarchar(1000))
insert into #Output
select selectionType,metricType,metric,selectionId,selectionName,percentage,0 as significance,groupsort,sortid,slideNumber, 0 isCategory,consumptionID,consumptionName 
from #finalOutputIntermediate where selectionType='Benchmark'

insert into #Output

select a.selectionType,a.metricType,a.metric,a.selectionId,a.selectionName,a.percentage,[dbo].[significanceValue](b.Percentage,b.samplesize,a.Percentage,c.samplesize) significance,a.groupsort,a.sortid,a.slidenumber,0 isCategory,a.consumptionID,a.consumptionName
from
#finalOutputIntermediate a
inner join
(
select a.selectionType,metricType,metric,a.selectionId,a.selectionName,percentage,0 as significance,groupsort,sortid,a.slideNumber,b.samplesize 
from #finalOutputIntermediate a
inner join #sample_size b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName
where a.selectionType='Benchmark'
) b
on a.MetricType=b.MetricType and a.Metric=b.Metric and a.selectionType!='Benchmark' and a.slidenumber=b.slidenumber
inner join #sample_size c on a.SelectionType=c.SelectionType and a.selectionId=c.selectionId and a.selectionName=c.selectionName

insert into #Output
select selectionType,metricType,AttributeName metric,selectionId,selectionName,percentage,0 as significance,groupsort,sortid,slideNumber ,0 isCategory,consumptionID,consumptionName
from #finalOutputIntermediate_Single where selectionType='Benchmark'

insert into #Output
select a.selectionType,a.metricType,a.attributeName metric,a.selectionId,a.selectionName,a.percentage,[dbo].[significanceValue](b.Percentage,b.samplesize,a.Percentage,c.samplesize) significance,a.groupsort,a.sortid,a.slidenumber,0 isCategory,a.consumptionID,a.consumptionName
from
#finalOutputIntermediate_Single a
inner join
(
select a.selectionType,metricType,metric,a.selectionId,a.selectionName,percentage,0 as significance,groupsort,sortid,a.slideNumber,b.samplesize 
from #finalOutputIntermediate_Single a
inner join #sample_size b on a.SelectionType=b.SelectionType and a.selectionId=b.selectionId and a.selectionName=b.selectionName
where a.selectionType='Benchmark'
) b
on a.MetricType=b.MetricType and a.Metric=b.Metric and a.selectionType!='Benchmark' and a.slidenumber=b.slidenumber
inner join #sample_size c on a.SelectionType=c.SelectionType and a.selectionId=c.selectionId and a.selectionName=c.selectionName

select selectionType,metricType,metric,selectionId,selectionName,isnull(percentage,0) percentage,isnull(significance,0) significance,groupsort,sortid,slidenumber,isCategory,consumptionID,consumptionName
from
(
select * from #Output
union all
select * from #output_equity_consumption
)  a
order by slideNumber,selectionType,selectionId,GroupSort,sortId


end

---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
																		/*CODE ENDS HERE*/
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


