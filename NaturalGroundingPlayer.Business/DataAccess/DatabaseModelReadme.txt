When making changes to the model, this must be added right before <EntityContainer> tag

      <Function Name="DbGetRatingValue" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="true" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo" ReturnType="real">
        <Parameter Name="height" Type="real" Mode="In" />
        <Parameter Name="depth" Type="real" Mode="In" />
        <Parameter Name="ratio" Type="real" Mode="In" />
      </Function>
      <Function Name="DbCompareValues" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="true" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo" ReturnType="bit">
        <Parameter Name="value1" Type="real" Mode="In" />
        <Parameter Name="compareOp" Type="int" Mode="In" />
        <Parameter Name="value2" Type="real" Mode="In" />
      </Function>
      <Function Name="substr" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="true" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo" ReturnType="nvarchar">
        <Parameter Name="text" Type="nvarchar" Mode="In" />
        <Parameter Name="startPos" Type="int" Mode="In" />
      </Function>

The auto-generated constructor must also be deleted.