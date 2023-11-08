------------------------------------------------------------------------------
                  -- Creare tabela ConditionSet --
------------------------------------------------------------------------------
IF NOT EXISTS
(
  SELECT 1 FROM sys.tables WHERE name = 'ConditionSet'
)
BEGIN

   CREATE TABLE [dbo].[ConditionSet]
  (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [FunctionType] NVARCHAR(128) NOT NULL,
    [A] DECIMAL(27, 10),
    [B] DECIMAL(27, 10),
    [IdRule] INT NOT NULL,
    [RowSignature] ROWVERSION NOT NULL,
	CONSTRAINT [PK_ConditionSet] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
  ) ON [PRIMARY]
END
GO

ALTER TABLE [dbo].[ConditionSet]  WITH CHECK ADD CONSTRAINT [FK_ConditionSet_Rule] FOREIGN KEY([IdRule])
    REFERENCES [dbo].[Rule] ([Id])
      ON DELETE CASCADE
GO

---------------------------------------------------------------------------------
              -- Creare procedura [ProcConditionSetSelect] --
---------------------------------------------------------------------------------
GO
IF EXISTS
(
  SELECT 1 FROM sys.objects
    WHERE [name] = 'ProcConditionSetSelect' AND [type] IN (N'P', N'PC')
)
BEGIN
  
  DROP PROCEDURE [dbo].[ProcConditionSetSelect]

END
GO

CREATE PROCEDURE [dbo].[ProcConditionSetSelect]
AS
BEGIN
  SELECT * FROM [ConditionSet] 
END
GO

---------------------------------------------------------------------------------
              -- Creare procedura [ProcConditionSethUpdate] --
---------------------------------------------------------------------------------
GO
IF EXISTS
(
  SELECT 1 FROM sys.objects
    WHERE [name] = 'ProcConditionSetUpdate' AND [type] IN (N'P', N'PC')
)
BEGIN
  
  DROP PROCEDURE [dbo].[ProcConditionSetUpdate]

END
GO

CREATE PROCEDURE [ProcConditionSetUpdate]
    @FunctionType NVARCHAR(128),
    @A DECIMAL(27, 10),
    @B DECIMAL(27, 10),
    @IdRule INT,
    @Id INT
AS
BEGIN

  UPDATE
      [ConditionSet]
    SET
      [FunctionType] = @FunctionType,
      [A] = @A,
      [B] = @B,
      [IdRule] = @IdRule
    WHERE
      [Id] = @Id
END
GO

---------------------------------------------------------------------------------
              -- Creare procedura [ProcConditionSetInsert] --
---------------------------------------------------------------------------------
GO
IF EXISTS
(
  SELECT 1 FROM sys.objects
    WHERE [name] = 'ProcConditionSetInsert' AND [type] IN (N'P', N'PC')
)
BEGIN
  
  DROP PROCEDURE [dbo].[ProcConditionSetInsert]

END
GO

CREATE PROCEDURE [ProcConditionSetInsert]
   @FunctionType NVARCHAR(128),
    @A DECIMAL(27, 10),
    @B DECIMAL(27, 10),
    @IdRule INT,
    @Id INT OUT
AS
BEGIN

  INSERT INTO [ConditionSet]
  (
    [FunctionType],
    [A],
    [B],
    [IdRule]
  )
  VALUES
  (
    @FunctionType,
    @A,
    @B,
    @IdRule
  )

  SET @Id = @@IDENTITY

END
GO

---------------------------------------------------------------------------------
              -- Creare procedura [ProcConditionSetDelete] --
---------------------------------------------------------------------------------
GO
IF EXISTS
(
  SELECT 1 FROM sys.objects
    WHERE [name] = 'ProcConditionSetDelete' AND [type] IN (N'P', N'PC')
)
BEGIN
  
  DROP PROCEDURE [dbo].[ProcConditionSetDelete]

END
GO

CREATE PROCEDURE [dbo].[ProcConditionSetDelete]
  @Id INT
AS
BEGIN

  DELETE FROM
      [ConditionSet] 
    WHERE
      [Id] = @Id
END
GO

-----------------------------------------------------------------------------------
-- Adaugare HasConditionSet pe Rule
-----------------------------------------------------------------------------------
GO
IF NOT EXISTS
(
  SELECT * FROM sys.columns
      WHERE Name = N'HasConditionSet' AND Object_ID = Object_ID(N'Rule')
)
BEGIN

  ALTER TABLE [Rule]
    ADD [HasConditionSet] BIT

END
GO

ALTER PROCEDURE [dbo].[ProcRuleInsert]
  @Guid UNIQUEIDENTIFIER,
  @IdWindowAccessory INT,
  @IdWindowHandwork INT,
  @IdWindowProcessingOperation INT,
  @IdWindowCustomMessage INT,
  @IdWindowTechnicalParameter INT,
  @IdAccessory1 INT,
  @IdAccessory2 INT,
  @IdHandwork1 INT,
  @IdProcessingOperation1 INT,
  @IdCustomMessage1 INT,
  @IdTechnicalParameter1 INT,
  @SeriesCode NVARCHAR(128),
  @Section1Code NVARCHAR(128),
  @Section2Code NVARCHAR(128),
  @Quantity DECIMAL(27,10),
  @Value DECIMAL(27, 10),
  @A DECIMAL(27,10),
  @B DECIMAL(27,10),
  @C DECIMAL(27,10),
  @GasketSpace DECIMAL(27, 10),
  @IdRange INT,
  @IdRuleTemplate INT,
  @StatusType NVARCHAR(128),
  @OpeningType NVARCHAR(128),
  @StructureType NVARCHAR(128),
  @OpeningSide NVARCHAR(128),
  @SectionType NVARCHAR(128),
  @CuttingType NVARCHAR(128),
  @JoinType NVARCHAR(128),
  @WingType NVARCHAR(128),
  @PanelType NVARCHAR(128),
  @ShapeTypeGuid UNIQUEIDENTIFIER,
  @BendType NVARCHAR(128),
  @ReinforcementType NVARCHAR(128),
  @ColorCombinationType NVARCHAR(128),
  @AlignmentType NVARCHAR(128),
  @AngleType NVARCHAR(128),
  @RuleFunction NVARCHAR(128),
  @SectionOrientation NVARCHAR(128),
  @SectionPosition NVARCHAR(128),
  @SectionFacePosition NVARCHAR(128),
  @PosX DECIMAL(27, 10),
  @FillingType NVARCHAR(128),
  @CustomProfile NVARCHAR(128),
  @SlidingTrackCount INT,
  @LeafCount INT,
  @SlidingWingType INT,
  @SlidingTrackNumber INT,
  @Description NVARCHAR(256),
  @GeneratorFace NVARCHAR(128),
  @Tag NVARCHAR(32),
  @GlassCode NVARCHAR(128),
  @MaterialSpeciesCode NVARCHAR(128),
  @HasConditionSet BIT,
  @Id INT OUT
AS
BEGIN
  INSERT INTO [Rule]
  (
    [Guid],
    [IdWindowAccessory],
    [IdWindowHandwork],
    [IdWindowProcessingOperation],
    [IdWindowCustomMessage],
    [IdWindowTechnicalParameter], 
    [IdAccessory1],
    [IdAccessory2],
    [IdHandwork1],
    [IdProcessingOperation1],
    [IdCustomMessage1], 
    [IdTechnicalParameter1],
    [SeriesCode],
    [Section1Code],
    [Section2Code],
    [Quantity],
    [Value],
    [A],
    [B],
    [C],
    [GasketSpace],
    [IdRange],
    [IdRuleTemplate],
    [SectionType],
    [CuttingType],
    [WingType],
    [JoinType],
    [StatusType],
    [OpeningType],
    [StructureType],
    [OpeningSide],
    [PanelType],
    [ShapeTypeGuid],
    [BendType],
    [ReinforcementType],
    [ColorCombinationType],
    [AlignmentType],
    [AngleType],
    [RuleFunction],
    [SectionOrientation],
    [SectionPosition],
    [SectionFacePosition],
    [PosX],
    [FillingType],
    [CustomProfile],
    [SlidingTrackCount],
    [LeafCount],
    [SlidingWingType],
    [Description],
    [SlidingTrackNumber],
    [GeneratorFace],
    [Tag],
    [GlassCode],
    [MaterialSpeciesCode],
    [HasConditionSet]
  )
  VALUES
  (
    @Guid,
    @IdWindowAccessory,
    @IdWindowHandwork,
    @IdWindowProcessingOperation,
    @IdWindowCustomMessage, 
    @IdWindowTechnicalParameter,
    @IdAccessory1,
    @IdAccessory2,
    @IdHandwork1,
    @IdProcessingOperation1,
    @IdCustomMessage1, 
    @IdTechnicalParameter1,
    @SeriesCode,
    @Section1Code,
    @Section2Code,
    @Quantity,
    @Value,
    @A,
    @B,
    @C,
    @GasketSpace,
    @IdRange,
    @IdRuleTemplate,
    @SectionType,
    @CuttingType,
    @WingType,
    @JoinType,
    @StatusType,
    @OpeningType,
    @StructureType,
    @OpeningSide,
    @PanelType,
    @ShapeTypeGuid,
    @BendType,
    @ReinforcementType,
    @ColorCombinationType,
    @AlignmentType,
    @AngleType,
    @RuleFunction,
    @SectionOrientation,
    @SectionPosition,
    @SectionFacePosition,
    @PosX,
    @FillingType,
    @CustomProfile,
    @SlidingTrackCount,
    @LeafCount,
    @SlidingWingType,
    @Description,
    @SlidingTrackNumber,
    @GeneratorFace,
    @Tag,
    @GlassCode,
    @MaterialSpeciesCode,
    @HasConditionSet
  )

  SET @Id = @@IDENTITY  
END
GO

ALTER PROCEDURE [dbo].[ProcRuleUpdate]
  @Guid UNIQUEIDENTIFIER,
  @IdWindowAccessory INT,
  @IdWindowHandwork INT,
  @IdWindowProcessingOperation INT,
  @IdWindowCustomMessage INT,
  @IdWindowTechnicalParameter INT,
  @IdAccessory1 INT,
  @IdAccessory2 INT,
  @IdHandwork1 INT,
  @IdProcessingOperation1 INT,
  @IdCustomMessage1 INT,
  @IdTechnicalParameter1 INT,
  @SeriesCode NVARCHAR(128),
  @Section1Code NVARCHAR(128),
  @Section2Code NVARCHAR(128),
  @Quantity DECIMAL(27,10),
  @Value DECIMAL(27, 10),
  @A DECIMAL(27,10),
  @B DECIMAL(27,10),
  @C DECIMAL(27,10),
  @GasketSpace DECIMAL(27, 10),
  @IdRange INT,
  @IdRuleTemplate INT,
  @SectionType NVARCHAR(128),
  @CuttingType NVARCHAR(128),
  @JoinType NVARCHAR(128),
  @WingType NVARCHAR(128),
  @StatusType NVARCHAR(128),
  @OpeningType NVARCHAR(128),
  @StructureType NVARCHAR(128),
  @OpeningSide NVARCHAR(128),
  @PanelType NVARCHAR(128),
  @ShapeTypeGuid UNIQUEIDENTIFIER,
  @BendType NVARCHAR(128),
  @ReinforcementType NVARCHAR(128),
  @ColorCombinationType NVARCHAR(128),
  @AlignmentType NVARCHAR(128),
  @AngleType NVARCHAR(128),
  @RuleFunction NVARCHAR(128),
  @SectionOrientation NVARCHAR(128),
  @SectionPosition NVARCHAR(128),
  @SectionFacePosition NVARCHAR(128),
  @PosX DECIMAL(27, 10),
  @FillingType NVARCHAR(128),
  @CustomProfile NVARCHAR(128),
  @SlidingTrackCount INT,
  @LeafCount INT,
  @SlidingWingType INT,
  @Description NVARCHAR(256),
  @SlidingTrackNumber INT,
  @GeneratorFace NVARCHAR(128),
  @Tag NVARCHAR(32),
  @GlassCode NVARCHAR(128),
  @MaterialSpeciesCode NVARCHAR(128),
  @HasConditionSet BIT,
  @Id INT
AS
BEGIN

  UPDATE
      [Rule]
    SET
      [Guid] = @Guid,
      [IdWindowAccessory] = @IdWindowAccessory,
      [IdWindowHandwork] = @IdWindowHandwork,
      [IdWindowProcessingOperation] = @IdWindowProcessingOperation,
      [IdWindowCustomMessage] = @IdWindowCustomMessage,
      [IdWindowTechnicalParameter] = @IdWindowTechnicalParameter,
      [IdAccessory1] = @IdAccessory1,
      [IdAccessory2] = @IdAccessory2,
      [IdHandwork1] = @IdHandwork1,
      [IdProcessingOperation1] = @IdProcessingOperation1,
      [IdCustomMessage1] = @IdCustomMessage1, 
      [IdTechnicalParameter1] = @IdTechnicalParameter1, 
      [SeriesCode] = @SeriesCode,
      [Section1Code] = @Section1Code,
      [Section2Code] = @Section2Code,
      [Quantity] = @Quantity,
      [Value] = @Value,
      [A] = @A,
      [B] = @B,
      [C] = @C,
      [GasketSpace] = @GasketSpace,
      [IdRange] = @IdRange,
      [IdRuleTemplate] = @IdRuleTemplate,
      [SectionType] = @SectionType,
      [CuttingType] = @CuttingType,
      [JoinType] = @JoinType,
      [WingType] = @WingType,
      [StatusType] = @StatusType,
      [OpeningType] = @OpeningType,
      [StructureType] = @StructureType,
      [OpeningSide] = @OpeningSide,
      [PanelType] = @PanelType,
      [ShapeTypeGuid] = @ShapeTypeGuid,
      [BendType] = @BendType,
      [ReinforcementType] = @ReinforcementType,
      [ColorCombinationType] = @ColorCombinationType,
      [AlignmentType] = @AlignmentType,
      [AngleType] = @AngleType,
      [RuleFunction] = @RuleFunction,
      [SectionOrientation] = @SectionOrientation,
      [SectionPosition] = @SectionPosition,
      [SectionFacePosition] = @SectionFacePosition,
      [PosX] = @PosX,
      [FillingType] = @FillingType,
      [CustomProfile] = @CustomProfile,
      [SlidingTrackCount] = @SlidingTrackCount,
      [LeafCount] = @LeafCount,
      [SlidingWingType] = @SlidingWingType,
      [Description] = @Description,
      [SlidingTrackNumber] = @SlidingTrackNumber,
      [GeneratorFace] = @GeneratorFace,
      [Tag] = @Tag,
      [GlassCode] = @GlassCode,
      [MaterialSpeciesCode] = @MaterialSpeciesCode,
      [HasConditionSet] = @HasConditionSet
    WHERE
      [Id] = @Id

END
GO
