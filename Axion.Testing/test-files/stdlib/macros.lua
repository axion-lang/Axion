macro until-statement()
    return ({{ 
            
            while not$conditiondo
                $scope
            end}})

macro post-condition-loop()
    if syntax[2]=="while"
        condition = {{ 
            not$condition
        }}
    return ({{ 
            
            while true do
                $scope
                if $condition
                    break
            end}})

macro unless-statement()
    return ({{ 
            
            if not$condition
                $scope
            else
                $else-scope
    }})

macro times-statement()
    counter-name
    return ({{ 
            
            $counter-name=0
            while $counter-name<$counter-enddo
                $scope
                $counter-name++
            end}})

macro for-in-statement()

macro for-index-statement()
    return ({{ 
            
            $init
            while $conditiondo
                $scope
                $step
            end}})

macro raise-statement()

macro match-expression()

macro object-initializer-expression()

macro list-init-expression()

macro map-init-expression()

macro set-init-expression()

macro with-statement()
    if exprisName
        name = value
    else
        init = expr
        name = expr.Left
    
    return ({{ 
            
            $init
            try
                
                $scope
            
            finally
                
                $name.destroy()
    }})
